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
            var currentDecryptedUser = new User
            {
                Id = currentEncryptedUser.Id,
                UserName = AesHelper.Decrypt(currentEncryptedUser.CitizenIdentificationNumber.ToString(), currentEncryptedUser.IvKey, currentEncryptedUser.StaticKey),
                CitizenIdentificationNumber = currentEncryptedUser.CitizenIdentificationNumber.ToString(),
                Adress = AesHelper.Decrypt(currentEncryptedUser.Adress, currentEncryptedUser.IvKey, currentEncryptedUser.StaticKey),
                ATM = AesHelper.Decrypt(currentEncryptedUser.ATM.ToString(), currentEncryptedUser.IvKey, currentEncryptedUser.StaticKey),
                Birthday = AesHelper.Decrypt(currentEncryptedUser.Birthday.ToString(), currentEncryptedUser.IvKey, currentEncryptedUser.StaticKey),
                Email = AesHelper.Decrypt(currentEncryptedUser.Email, currentEncryptedUser.IvKey, currentEncryptedUser.StaticKey),
                Name = currentEncryptedUser.Name,
                PhoneNumber = AesHelper.Decrypt(currentEncryptedUser.PhoneNumber.ToString(), currentEncryptedUser.IvKey, currentEncryptedUser.StaticKey),
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
            var currentDecryptedUser = new User
            {
                UserName = AesHelper.Decrypt(encryptedUser.CitizenIdentificationNumber.ToString(), encryptedUser.IvKey, encryptedUser.StaticKey),
                CitizenIdentificationNumber = encryptedUser.CitizenIdentificationNumber.ToString(),
                Adress = AesHelper.Decrypt(encryptedUser.Adress, encryptedUser.IvKey, encryptedUser.StaticKey),
                ATM = AesHelper.Decrypt(encryptedUser.ATM.ToString(), encryptedUser.IvKey, encryptedUser.StaticKey),
                Birthday = AesHelper.Decrypt(encryptedUser.Birthday.ToString(), encryptedUser.IvKey, encryptedUser.StaticKey),
                Email = AesHelper.Decrypt(encryptedUser.Email, encryptedUser.IvKey, encryptedUser.StaticKey),
                Name = encryptedUser.Name,
                PhoneNumber = AesHelper.Decrypt(encryptedUser.PhoneNumber.ToString(), encryptedUser.IvKey, encryptedUser.StaticKey),
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
