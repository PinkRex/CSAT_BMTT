using CSAT_BMTT.Models;
using CSAT_BMTT.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CSAT_BMTT.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _config;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid infomation!";
                return RedirectToAction("Register");
            }

            var existingUser = await _userManager.FindByNameAsync(model.CitizenIdentificationNumber.ToString());
            if (existingUser != null)
            {
                TempData["ErrorMessage"] = "CitizenIdentificationNumber is already taken!";
                return RedirectToAction("Register");
            }

            var pinCodesKey = model.PinCode + model.CitizenIdentificationNumber[..10];
            var pinCodeIv = string.Concat(Enumerable.Repeat(model.PinCode, 9)) + model.CitizenIdentificationNumber[..10];

            var staticKey = AesHelper.GenerateAesStaticKey(16);
            var ivKey = AesHelper.GenerateAesStaticKey(64);

            Dictionary<string, string> keys = RsaHelper.GenerateKey();
            string publicKey = keys["public_key"];
            string privateKey = keys["private_key"];

            var user = new User
            {
                PrivateKey = AesHelper.Encrypt(privateKey, pinCodeIv, pinCodesKey), 
                StaticKey = RsaHelper.Encrypt(staticKey, publicKey),
                IvKey = RsaHelper.Encrypt(ivKey, publicKey),
                PublicKey = publicKey,
                UserName = AesHelper.Encrypt(model.CitizenIdentificationNumber.ToString(), ivKey, staticKey),
                CitizenIdentificationNumber = model.CitizenIdentificationNumber.ToString(),
                Adress = AesHelper.Encrypt(model.Adress, ivKey, staticKey),
                ATM = AesHelper.Encrypt(model.ATM.ToString(), ivKey, staticKey),
                Birthday = AesHelper.Encrypt(model.Birthday.ToString(), ivKey, staticKey),
                Email = AesHelper.Encrypt(model.Email, ivKey, staticKey),
                Name = model.Name,
                PhoneNumber = AesHelper.Encrypt(model.PhoneNumber.ToString(), ivKey, staticKey),
            };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = GenerateJwtToken(user);

                Response.Cookies.Append("access_token", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddHours(1)
                });
                return RedirectToAction("Index", "Users");
            }
            TempData["ErrorMessage"] = "Invalid infomation!";
            return RedirectToAction("Register");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.CitizenIdentificationNumber.ToString() == model.CitizenIdentificationNumber);
            if (user == null || !(await _userManager.CheckPasswordAsync(user, model.Password)))
            {
                TempData["ErrorMessage"] = "Invalid username or password!";
                return RedirectToAction("Login");
            }

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.UtcNow.AddHours(1)
            });

            return RedirectToAction("Index", "Users");
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            Response.Cookies.Delete("access_token");
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
        };

            var creds = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}