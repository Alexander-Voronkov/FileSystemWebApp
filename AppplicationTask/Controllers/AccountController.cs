using AppplicationTask.Authentication.Entities;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AppplicationTask.Models;
using Microsoft.EntityFrameworkCore;
using AppplicationTask.Data.Contexts;
using System.Text;
using System.Security.Cryptography;
using AppplicationTask.Data.Entities;
using AppplicationTask.Utils;

namespace AppplicationTask.Controllers
{
    public class AccountController : Controller
    {
        private readonly FileSystemContext _context;
        private readonly IHashService _hash;
        public AccountController(FileSystemContext context, IHashService hash)
        {
            _context = context;
            _hash = hash;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)    
            // метод входу в акаунт
        {
            if (ModelState.IsValid)
            {
                User? user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email
                        && u.Password == _hash.HashPassword(model.Password));
                if (user != null)
                {
                    await Authenticate(model.Email);
                    return RedirectToAction("Root", "Home");
                }
                ModelState.AddModelError("", "Неправильні логін або пароль!");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)   
            // метод реєстрації
        {
            if (ModelState.IsValid)
            {
                User? user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null)
                {
                    User userToAdd = new User
                    {
                        Email = model.Email,
                        Password = _hash.HashPassword(model.Password),
                    };
                    await _context.Users.AddAsync(userToAdd);
                    await Authenticate(model.Email);
                    Folder? root = null;
                    root = new AppplicationTask.Data.Entities.Folder()
                    {
                        Name = "Root",
                        ParentFolder = root,
                        Owner = userToAdd
                    };
                    await _context.Folders.AddAsync(root);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Root", "Home");
                }
                ModelState.AddModelError("", "Неправильні логін або пароль!");
            }
            return View(model);
        }

        private async Task Authenticate(string userName) 
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, userName)
            };
            ClaimsIdentity id = new ClaimsIdentity(
                claims: claims,
                authenticationType: "ApplicationCookie",
                nameType: ClaimsIdentity.DefaultNameClaimType,
                roleType: ClaimsIdentity.DefaultRoleClaimType
            );
            await HttpContext.SignInAsync(
            scheme: CookieAuthenticationDefaults.AuthenticationScheme,
            principal: new ClaimsPrincipal(id)
            );
        }
        public async Task<IActionResult> Logout()  
            // метод виходу з акаунту
        {
            await HttpContext.SignOutAsync(
            scheme: CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login), "Account");
        }
        
    }
}
