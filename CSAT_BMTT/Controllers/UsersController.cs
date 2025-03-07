using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CSAT_BMTT.Data;
using CSAT_BMTT.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var currentUser = await _context.User
                .FirstOrDefaultAsync(u => u.Id.ToString() == currentID);
            var users = await _context.User.Where(u => u.Id.ToString() != currentID).ToListAsync();

            if (!String.IsNullOrEmpty(searchString))
            {
                users = users.Where(s => s.Name!.ToUpper().Contains(searchString.ToUpper())).ToList();
            }

            var model = new UsersViewModel
            {
                CurrentUser = currentUser,
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

            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
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
                user.Adress = userModel.Adress;
                user.ATM = userModel.ATM;
                user.Birthday = userModel.Birthday;
                user.Email = userModel.Email;
                user.Name = userModel.Name;
                user.PhoneNumber = userModel.PhoneNumber;
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
