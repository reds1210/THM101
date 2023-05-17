using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using WebApplication4.Models;
using System.Text.Json;
using WebApplication4.Services;

namespace WebApplication4.Controllers
{
    public class UserController : Controller
    {
        private readonly Thm101Context _db;
        private readonly EncryptService encrypt;

        public UserController(Thm101Context context, EncryptService encrypt)
        {
            _db = context;
            this.encrypt = encrypt;
        }
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var user = _db.Users.FirstOrDefault(x => x.Account == model.Account);
            if (user != null)
            {
                ViewBag.Error = "帳號已經存在";
                return View("Register");
            }

            _db.Users.Add(new User()
            {
                Account = model.Account,
                Password = model.Password,
                Name = model.Name,
                Role = "User",
                IsActive = false
            });
            _db.SaveChanges();
            //寄信
            var obj = new AesValidationDto(model.Account,DateTime.Now.AddDays(3));
            var jString = JsonSerializer.Serialize(obj);
            var code = encrypt.AesEncryptToBase64(jString);
            

            var mail = new MailMessage()
            {
                From = new MailAddress("thm101team66@gmail.com"),
                Subject = "啟用網站驗證",
                Body = @$"請點這<a src='https://localhost:7157/user/enable?code={code}'>這裡</a>來啟用你的帳號",
                IsBodyHtml = true,
                BodyEncoding = Encoding.UTF8,
            };
            mail.To.Add(new MailAddress(model.Account));
            try
            {
                using (var sm = new SmtpClient("smtp.gmail.com", 587)) //465 ssl
                {
                    sm.EnableSsl = true;
                    sm.Credentials = new NetworkCredential("thm101team66@gmail.com", "kfwpggrhucetwqsd");
                    sm.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return View();


        }
        public async Task<IActionResult> Enable(string code)
        {
            var str = encrypt.AesDecryptToString(code);
            var obj = JsonSerializer.Deserialize<AesValidationDto>(str);
            if (DateTime.Now > obj.ExpiredDate) {
                return BadRequest("過期");
            }
            var user = _db.Users.FirstOrDefault(x => x.Account == obj.Account);
            if (user != null)
            {
                user.IsActive = true;
                _db.SaveChanges();
            }
            
            return Ok($@"code:{code}  str:{str}");
        }


        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            var user = _db.Users.FirstOrDefault(x => x.Account == model.Account &&
            x.Password == model.Password);
            if (user == null)
            {
                ViewBag.Error = "帳號密碼錯誤";
                return View("login");
            }
            //通行證 證件 身分證 護照 駕照 戶口名簿
            //姓名:xxx 證號:A123456789 父:XXX
            var claims = new List<Claim>() {
                 new Claim(ClaimTypes.Name, user.Name),
                 new Claim(ClaimTypes.Role, user.Role),
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            await HttpContext.SignInAsync(claimsPrincipal);
            return RedirectToAction("Index", "home");
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "home");
        }
    }
}
