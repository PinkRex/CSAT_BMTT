using CSAT_BMTT.Data;
using CSAT_BMTT.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Transactions;

namespace CSAT_BMTT.Controllers
{
    [Route("api/accesspermissions")]
    public class AccessPermissionsController : Controller
    {
        private readonly CSAT_BMTTContext _context;
        public AccessPermissionsController(CSAT_BMTTContext context) => _context = context;

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePermissionRequest([FromForm] int targetId)
        {
            if (_context.AccessPermission == null)
            {
                return Problem("Entity set 'CSAT_BMTTContext.AccessPermission' is null.");
            }

            var requestorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _context.User.FindAsync(int.Parse(requestorId));
            var targetUser = await _context.User.FindAsync(targetId);

            if (currentUser == null || targetUser == null)
            {
                TempData["ErrorMessage"] = "User does not exist!";
            }
            var requestorPublicKey = currentUser.PublicKey;

            //var encryptedTargetIvKey = targetUser.IvKey;
            //var encryptedTargetStaticKey = targetUser.StaticKey;

            var accessPermission = new AccessPermissionModel
            {
                RequestorID = int.Parse(requestorId),
                TargetId = targetId,
                RequestorPublicKey = requestorPublicKey,
                //TargetIvKey = encryptedTargetIvKey,
                //TargetStaticKey = encryptedTargetStaticKey,
                Status = AccessPermissionStatus.Pending
            };

            _context.AccessPermission.Add(accessPermission);
            await _context.SaveChangesAsync();

            return View("Index" ,accessPermission);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> RequestList()
        {
            var requestorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // Lấy danh sách các yêu cầu mà user hiện tại đã gửi
            var requestSentList = await _context.AccessPermission
                .Where(ap => ap.RequestorID == requestorId)
                .Select(ap => new RequestModel
                {
                    CitizenIdentificationNumber = _context.User.Where(u => u.Id == ap.TargetId)
                                                               .Select(u => u.CitizenIdentificationNumber)
                                                               .FirstOrDefault(),
                    CitizenName = _context.User.Where(u => u.Id == ap.TargetId)
                                               .Select(u => u.Name)
                                               .FirstOrDefault(),
                    Status = ap.Status.ToString()
                })
                .ToListAsync();

            // Lấy danh sách các yêu cầu mà user hiện tại nhận được
            var requestReceivedList = await _context.AccessPermission
                .Where(ap => ap.TargetId == requestorId)
                .Select(ap => new RequestModel
                {
                    CitizenIdentificationNumber = _context.User.Where(u => u.Id == ap.RequestorID)
                                                               .Select(u => u.CitizenIdentificationNumber)
                                                               .FirstOrDefault(),
                    CitizenName = _context.User.Where(u => u.Id == ap.RequestorID)
                                               .Select(u => u.Name)
                                               .FirstOrDefault(),
                    Status = ap.Status.ToString()
                })
                .ToListAsync();

            // Trả về dữ liệu dưới dạng ViewModel
            var model = new RequestViewModel
            {
                RequestSentList = requestSentList,
                RequestRecievedList = requestReceivedList
            };

            return View("RequestList", model);
        }
    }
}
