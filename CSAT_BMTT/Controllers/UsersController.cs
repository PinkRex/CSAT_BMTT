using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSAT_BMTT.Data;
using CSAT_BMTT.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using CSAT_BMTT.Utils;

namespace CSAT_BMTT.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly CSAT_BMTTContext _context;

        public UsersController(CSAT_BMTTContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index(string? searchString)
        {
            if (_context.User == null)
            {
                return Problem("Entity set 'CSAT_BMTTContext.User' is null.");
            }

            var currentID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentEncryptedUser = await _context.User
                .FirstOrDefaultAsync(u => u.Id.ToString() == currentID);

            var pinCode = "190103";
            var pinCodesKey = pinCode + currentEncryptedUser.CitizenIdentificationNumber[..10];
            var pinCodeIv = string.Concat(Enumerable.Repeat(pinCode, 9)) + currentEncryptedUser.CitizenIdentificationNumber[..10];

            var decryptedPrivateKey = AesHelper.Decrypt(currentEncryptedUser.PrivateKey, pinCodeIv, pinCodesKey);
            var decryptedIvKey = RsaHelper.Decrypt(currentEncryptedUser.IvKey, decryptedPrivateKey);
            var decryptedStaticKey = RsaHelper.Decrypt(currentEncryptedUser.StaticKey, decryptedPrivateKey);

            var currentDecryptedUser = new User
            {
                Id = currentEncryptedUser.Id,
                UserName = AesHelper.Decrypt(currentEncryptedUser.CitizenIdentificationNumber.ToString(), decryptedIvKey, decryptedStaticKey),
                CitizenIdentificationNumber = currentEncryptedUser.CitizenIdentificationNumber.ToString(),
                Adress = AesHelper.Decrypt(currentEncryptedUser.Adress, decryptedIvKey, decryptedStaticKey),
                ATM = AesHelper.Decrypt(currentEncryptedUser.ATM.ToString(), decryptedIvKey, decryptedStaticKey),
                Birthday = AesHelper.Decrypt(currentEncryptedUser.Birthday.ToString(), decryptedIvKey, decryptedStaticKey),
                Email = AesHelper.Decrypt(currentEncryptedUser.Email, decryptedIvKey, decryptedStaticKey),
                Name = currentEncryptedUser.Name,
                PhoneNumber = AesHelper.Decrypt(currentEncryptedUser.PhoneNumber.ToString(), decryptedIvKey, decryptedStaticKey),
            };

            var users = await _context.User.Where(u => u.Id.ToString() != currentID).ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Name!.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            var model = new UsersViewModel
            {
                CurrentUser = currentDecryptedUser,
                UsersList = users
            };

            return View(model);
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var encryptedUser = await _context.User.FindAsync(id);
            if (encryptedUser == null)
            {
                return NotFound();
            }

            // Thêm check quyền

            var pinCode = "190103";
            var pinCodesKey = pinCode + encryptedUser.CitizenIdentificationNumber[..10];
            var pinCodeIv = string.Concat(Enumerable.Repeat(pinCode, 9)) + encryptedUser.CitizenIdentificationNumber[..10];

            var decryptedPrivateKey = AesHelper.Decrypt(encryptedUser.PrivateKey, pinCodeIv, pinCodesKey);
            var decryptedIvKey = RsaHelper.Decrypt(encryptedUser.IvKey, decryptedPrivateKey);
            var decryptedStaticKey = RsaHelper.Decrypt(encryptedUser.StaticKey, decryptedPrivateKey);

            var currentDecryptedUser = new User
            {
                UserName = AesHelper.Decrypt(encryptedUser.CitizenIdentificationNumber, decryptedIvKey, decryptedStaticKey),
                CitizenIdentificationNumber = encryptedUser.CitizenIdentificationNumber,
                Adress = AesHelper.Decrypt(encryptedUser.Adress, decryptedIvKey, decryptedStaticKey),
                ATM = AesHelper.Decrypt(encryptedUser.ATM, decryptedIvKey, decryptedStaticKey),
                Birthday = AesHelper.Decrypt(encryptedUser.Birthday, decryptedIvKey, decryptedStaticKey),
                Email = AesHelper.Decrypt(encryptedUser.Email, decryptedIvKey, decryptedStaticKey),
                Name = encryptedUser.Name,
                PhoneNumber = AesHelper.Decrypt(encryptedUser.PhoneNumber, decryptedIvKey, decryptedStaticKey),
            };

            return View(currentDecryptedUser);
        }

        [HttpPost("edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] User userModel)
        {
            if (id != userModel.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var user = await _context.User.FindAsync(id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Adress = AesHelper.Encrypt(userModel.Adress, user.IvKey, user.StaticKey);
                user.ATM = AesHelper.Encrypt(userModel.ATM, user.IvKey, user.StaticKey);
                user.Birthday = AesHelper.Encrypt(userModel.Birthday, user.IvKey, user.StaticKey);
                user.Email = AesHelper.Encrypt(userModel.Email, user.IvKey, user.StaticKey);
                user.Name = userModel.Name;
                user.PhoneNumber = AesHelper.Encrypt(userModel.PhoneNumber, user.IvKey, user.StaticKey);
                try
                {
                    _context.Update(user);
                    await _context.SaveChangesAsync();
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
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.User
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost("delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user != null)
            {
                _context.User.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}
