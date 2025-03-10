using CSAT_BMTT.Data;
using CSAT_BMTT.Hubs;
using CSAT_BMTT.Models;
using CSAT_BMTT.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Transactions;

namespace CSAT_BMTT.Controllers
{
    [Route("api/accesspermissions")]
    public class AccessPermissionsController : Controller
    {
        private readonly CSAT_BMTTContext _context;
        private readonly IHubContext<PermissionHub> _hubContext;

        public AccessPermissionsController(CSAT_BMTTContext context, IHubContext<PermissionHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpPost("CreatePermissionRequest")]
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
            bool isDuplicateRequest = await _context.AccessPermission
                .AnyAsync(ap => ap.RequestorID == int.Parse(requestorId) && ap.TargetId == targetId && ap.Status == AccessPermissionStatus.Pending);

            if (isDuplicateRequest)
            {
                TempData["ErrorMessage"] = "You have already sent a request to this user!";
                return RedirectToAction("Index", "Users");
            }

            var requestorPublicKey = currentUser.PublicKey;
            var accessPermission = new AccessPermissionModel
            {
                RequestorID = int.Parse(requestorId),
                TargetId = targetId,
                RequestorPublicKey = requestorPublicKey,
                Status = AccessPermissionStatus.Pending
            };

            _context.AccessPermission.Add(accessPermission);
            await _context.SaveChangesAsync();
            await _hubContext.Clients.All.SendAsync("UpdateRequests");

            return View("Index", accessPermission);
        }

        [HttpGet("requests")]
        public async Task<IActionResult> RequestList()
        {
            var requestorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var requestSentList = await _context.AccessPermission
                .Where(ap => ap.RequestorID == requestorId)
                .Select(ap => new RequestModel
                {
                    Id = ap.Id,
                    CitizenIdentificationNumber = _context.User.Where(u => u.Id == ap.TargetId)
                                                               .Select(u => u.CitizenIdentificationNumber)
                                                               .FirstOrDefault(),
                    CitizenName = _context.User.Where(u => u.Id == ap.TargetId)
                                               .Select(u => u.Name)
                                               .FirstOrDefault(),
                    Status = ap.Status.ToString()
                })
                .ToListAsync();
            var requestReceivedList = await _context.AccessPermission
                .Where(ap => ap.TargetId == requestorId)
                .Select(ap => new RequestModel
                {
                    Id = ap.Id,
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

        [HttpGet("requests/data")]
        public async Task<IActionResult> GetRequests()
        {
            var requestorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            var requestReceivedList = await _context.AccessPermission
                .Where(ap => ap.TargetId == requestorId)
                .Select(ap => new RequestModel
                {
                    CitizenIdentificationNumber = _context.User
                        .Where(u => u.Id == ap.RequestorID)
                        .Select(u => u.CitizenIdentificationNumber)
                        .FirstOrDefault(),
                    CitizenName = _context.User
                        .Where(u => u.Id == ap.RequestorID)
                        .Select(u => u.Name)
                        .FirstOrDefault(),
                    Status = ap.Status.ToString()
                })
                .ToListAsync();

            return Ok(requestReceivedList);
        }

        [HttpPost("ProcessRequest")]
        public async Task<IActionResult> ProcessRequest([FromForm] int? id, [FromForm] string pinCode, [FromForm] string isAccept)
        {
            if (isAccept == "1")
            {
                await ApproveRequest(id, pinCode);
            }
            return RedirectToAction("RequestList");
        }

        public async Task ApproveRequest(int? id, string pinCode)
        {
            var approverId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var approver = await _context.User.FindAsync(int.Parse(approverId));
            if (approver == null)
            {
                TempData["ErrorMessage"] = "User does not exist!";
                return;
            }

            var approverPinCodesKey = pinCode + approver.CitizenIdentificationNumber[..10];
            var approverPinCodeIv = string.Concat(Enumerable.Repeat(pinCode, 9)) + approver.CitizenIdentificationNumber[..10];
            var decryptedApproverPrivateKey = AesHelper.Decrypt(approver.PrivateKey, approverPinCodeIv, approverPinCodesKey);

            var approverIvKey = RsaHelper.Decrypt(approver.IvKey, decryptedApproverPrivateKey);
            var approverStaticKey = RsaHelper.Decrypt(approver.StaticKey, decryptedApproverPrivateKey);

            var accessPermission = await _context.AccessPermission.FindAsync(id);

            // Mã hóa với publicKey của requestor
            var encryptIvKey = RsaHelper.Encrypt(approverIvKey, accessPermission.RequestorPublicKey);
            var encryptStaticKey = RsaHelper.Encrypt(approverStaticKey, accessPermission.RequestorPublicKey);

            accessPermission.TargetIvKey = encryptIvKey;
            accessPermission.TargetStaticKey = encryptStaticKey;
            accessPermission.Status = AccessPermissionStatus.Approved;

            _context.AccessPermission.Update(accessPermission);
            await _context.SaveChangesAsync();
        }

        public void DeclineRequest([FromForm] int? id)
        {
            Console.WriteLine(id);
        }
    }
}
