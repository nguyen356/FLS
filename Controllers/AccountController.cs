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
            System.Diagnostics.Debug.WriteLine($"=== LOGIN DEBUG ===");
            System.Diagnostics.Debug.WriteLine($"Username: '{model.Username}'");
            System.Diagnostics.Debug.WriteLine($"Password: '{model.Password?.Length} chars'");

            if (!ModelState.IsValid)
            {
                System.Diagnostics.Debug.WriteLine("❌ Model invalid");
                return View(model);
            }

            var users = await _mongoDB.Users.Find(_ => true).ToListAsync();
            System.Diagnostics.Debug.WriteLine($"Total users in DB: {users.Count}");

            var user = await _mongoDB.Users.Find(u => u.Username == model.Username).FirstOrDefaultAsync();
            System.Diagnostics.Debug.WriteLine($"User found: {user != null}");
            if (user != null)
            {
                System.Diagnostics.Debug.WriteLine($"  Username: {user.Username}");
                System.Diagnostics.Debug.WriteLine($"  Role: {user.Role}");
                System.Diagnostics.Debug.WriteLine($"  Active: {user.IsActive}");
                System.Diagnostics.Debug.WriteLine($"  Password match: {VerifyPassword(model.Password, user.PasswordHash)}");
            }

            if (user != null && VerifyPassword(model.Password, user.PasswordHash))
            {
                System.Diagnostics.Debug.WriteLine("✅ MATCH - Setting session");
                HttpContext.Session.Clear();
                HttpContext.Session.SetString("UserId", user.Id ?? "");
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("Role", user.Role);

                // Force save
                await HttpContext.Session.CommitAsync();

                System.Diagnostics.Debug.WriteLine($"SESSION SAVED - Role: '{HttpContext.Session.GetString("Role")}'");
                TempData["Success"] = "Login successful!";
                return Redirect("/Admin");
            }

            System.Diagnostics.Debug.WriteLine("❌ LOGIN FAILED");
            ModelState.AddModelError("", $"Invalid: {model.Username}");
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