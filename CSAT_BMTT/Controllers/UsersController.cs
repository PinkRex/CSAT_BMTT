using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSAT_BMTT.Data;
using CSAT_BMTT.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CSAT_BMTT.Utils;
using CSAT_BMTT.DTOs;

namespace CSAT_BMTT.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly CSAT_BMTTContext _context;

        public UsersController(CSAT_BMTTContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Index(string? searchString)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'CSAT_BMTTContext.User' is null.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FindAsync(int.Parse(currentUserId));
            if (currentUser == null) return Unauthorized();

            var currentDecryptedUser = new User
            {
                Id = currentUser.Id,
                CitizenIdentificationNumber = currentUser.CitizenIdentificationNumber,
                Name = currentUser.Name
            };

            var users = await _context.User.Where(u => u.Id.ToString() != currentUserId).ToListAsync();

            if (!string.IsNullOrEmpty(searchString))
            {
                users = users.Where(u => u.Name.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            MaskUsersInfo(users);

            var model = new UsersViewModel
            {
                CurrentUser = currentDecryptedUser,
                UsersList = users
            };

            return View(model);
        }

        [HttpPost("ShowApprovedUsers")]
        public async Task<IActionResult> ShowApprovedUsers([FromForm] string pinCode)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'CSAT_BMTTContext.User' is null.");
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FindAsync(int.Parse(currentUserId));
            if (currentUser == null) return Unauthorized();

            var currentDecryptedUser = new User
            {
                Id = currentUser.Id,
                CitizenIdentificationNumber = currentUser.CitizenIdentificationNumber,
                Name = currentUser.Name
            };

            var users = await _context.User.Where(u => u.Id.ToString() != currentUserId).ToListAsync();
            var accessPermissions = await _context.AccessPermission
                .Where(ap => ap.RequestorID == int.Parse(currentUserId) && ap.Status == AccessPermissionStatus.Approved)
                .ToListAsync();
            var approvedUsersId = accessPermissions.Select(ap => ap.TargetId).ToList();

            // Danh sách các tài khoản sẽ được giải mã
            var approvedUsers = users.Where(u => !approvedUsersId.Contains(u.Id)).ToList();

            for (var i = 0; i < approvedUsers.Count; i++)
            {
                approvedUsers[i] = DecryptUser(approvedUsers[i], pinCode);
            }

            // Danh sách chỉ còn những tài khoản sẽ bị mask
            users.RemoveAll(u => approvedUsersId.Contains(u.Id));
            MaskUsersInfo(users);

            users.InsertRange(0, approvedUsers);

            var model = new UsersViewModel
            {
                CurrentUser = currentDecryptedUser,
                UsersList = users
            };

            return RedirectToAction("Index", model);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();
            MaskUserInfo(user);
            return View(user);
        }

        [HttpPost("edit/{id}")]
        [ActionName("EditPin")]
        public async Task<IActionResult> EditPin([FromForm] PinCodeDto pinCodeDto)
        {
            if (string.IsNullOrEmpty(pinCodeDto.PinCode)) return NotFound();

            var encryptedUser = await _context.User.FindAsync(pinCodeDto.Id);
            if (encryptedUser == null) return NotFound();

            // Thêm check quyền
            var currentDecryptedUser = DecryptUser(encryptedUser, pinCodeDto.PinCode);
            if (currentDecryptedUser != null)
            {
                return View("Edit", new UserDto(currentDecryptedUser, ""));
            }
            else
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost("edituser")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser([FromForm] UserDto userModel)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.User.FindAsync(userModel.Id);
                if (user == null) return NotFound();

                var isMatchPinCode = true;
                EncryptUser(user, userModel, isMatchPinCode);

                try
                {
                    if (isMatchPinCode)
                    {
                        _context.Update(user);
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(userModel);
        }

        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.User.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null) _context.User.Remove(user);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }

        private void MaskUsersInfo(List<User> users)
        {
            if (users == null || users.Count == 0) return;
            foreach (var user in users)
            {
                MaskUserInfo(user);
            }
        }

        private void MaskUserInfo(User user)
        {
            if (user == null) return;
            foreach (var prop in typeof(User).GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanWrite))
            {
                if (prop.Name != nameof(user.CitizenIdentificationNumber)
                    && prop.Name != nameof(user.Name))
                {
                    prop.SetValue(user, "********");
                }
            }
        }

        private (string IvKey, string StaticKey) DecryptKeys(User user, string pinCode)
        {
            var pinCodesKey = pinCode + user.CitizenIdentificationNumber[..10];
            var pinCodeIv = string.Concat(Enumerable.Repeat(pinCode, 9)) + user.CitizenIdentificationNumber[..10];
            var decryptedPrivateKey = AesHelper.Decrypt(user.PrivateKey, pinCodeIv, pinCodesKey);
            return
            (
                IvKey: RsaHelper.Decrypt(user.IvKey, decryptedPrivateKey),
                StaticKey: RsaHelper.Decrypt(user.StaticKey, decryptedPrivateKey)
            );
        }

        private User DecryptUser(User encryptedUser, string pinCode)
        {
            try
            {
                var decryptedKeys = DecryptKeys(encryptedUser, pinCode);
                return new User
                {
                    UserName = AesHelper.Decrypt(encryptedUser.CitizenIdentificationNumber, decryptedKeys.IvKey, decryptedKeys.StaticKey),
                    CitizenIdentificationNumber = encryptedUser.CitizenIdentificationNumber,
                    Adress = AesHelper.Decrypt(encryptedUser.Adress, decryptedKeys.IvKey, decryptedKeys.StaticKey),
                    ATM = AesHelper.Decrypt(encryptedUser.ATM, decryptedKeys.IvKey, decryptedKeys.StaticKey),
                    Birthday = AesHelper.Decrypt(encryptedUser.Birthday, decryptedKeys.IvKey, decryptedKeys.StaticKey),
                    Email = AesHelper.Decrypt(encryptedUser.Email, decryptedKeys.IvKey, decryptedKeys.StaticKey),
                    Name = encryptedUser.Name,
                    PhoneNumber = AesHelper.Decrypt(encryptedUser.PhoneNumber, decryptedKeys.IvKey, decryptedKeys.StaticKey),
                };
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Invalid PIN. Please try again.";
                return null;
            }
        }

        private void EncryptUser(User user, UserDto userModel, bool isMatchPinCode)
        {
            try
            {
                var decryptedKeys = DecryptKeys(user, userModel.PinCode);
                user.Adress = AesHelper.Encrypt(userModel.Adress, decryptedKeys.IvKey, decryptedKeys.StaticKey);
                user.ATM = AesHelper.Encrypt(userModel.ATM, decryptedKeys.IvKey, decryptedKeys.StaticKey);
                user.Birthday = AesHelper.Encrypt(userModel.Birthday, decryptedKeys.IvKey, decryptedKeys.StaticKey);
                user.Email = AesHelper.Encrypt(userModel.Email, decryptedKeys.IvKey, decryptedKeys.StaticKey);
                user.Name = userModel.Name;
                user.PhoneNumber = AesHelper.Encrypt(userModel.PhoneNumber, decryptedKeys.IvKey, decryptedKeys.StaticKey);
            }
            catch (Exception ex)
            {
                isMatchPinCode = false;
                TempData["ErrorMessage"] = "Invalid PIN. Please try again.";
            }
        }
    }
}
