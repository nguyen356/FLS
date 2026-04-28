using Microsoft.AspNetCore.Mvc;
using FlowerShop.Services;
using FlowerShop.Models;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace FlowerShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly MongoDBService _mongoDB;

        public AccountController(MongoDBService mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _mongoDB.Users.Find(u => u.Username == model.Username && u.IsActive).FirstOrDefaultAsync();
            if (user != null && VerifyPassword(model.Password, user.PasswordHash))
            {
                HttpContext.Session.SetString("UserId", user.Id ?? "");
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);
                TempData["Success"] = "Welcome back!";
                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid credentials");
            return View(model);
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid || model.Password != model.ConfirmPassword)
            {
                return View(model);
            }

            var exists = await _mongoDB.Users.Find(u => u.Username == model.Username || u.Email == model.Email).AnyAsync();
            if (exists)
            {
                ModelState.AddModelError("", "User already exists");
                return View(model);
            }

            var user = new ApplicationUserMongo
            {
                Username = model.Username,
                Email = model.Email,
                PasswordHash = HashPassword(model.Password),
                Role = model.Role
            };

            await _mongoDB.Users.InsertOneAsync(user);
            TempData["Success"] = "Registered! Please login.";
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Dashboard()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role)) return RedirectToAction("Login");

            var flowers = await _mongoDB.Flowers.Find(_ => true).ToListAsync();
            var allSales = await _mongoDB.Sales.Find(_ => true).ToListAsync();
            var recentSales = allSales.OrderByDescending(s => s.SaleDate).Take(10).ToList();

            var totalSales = allSales.Count;
            var totalRevenue = allSales.Sum(s => s.TotalPrice);

            ViewBag.Role = role;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Flowers = flowers;
            ViewBag.Sales = recentSales;
            ViewBag.TotalSales = totalSales;
            ViewBag.TotalRevenue = totalRevenue;
            return View();
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }

        private static bool VerifyPassword(string password, string hash) => HashPassword(password) == hash;
    }
}