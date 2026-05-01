using Microsoft.AspNetCore.Mvc;
using FlowerShop.Services;
using FlowerShop.Models;
using MongoDB.Driver;

namespace FlowerShop.Controllers
{
    public class AdminController : Controller
    {
        private readonly MongoDBService _mongoDB;

        public AdminController(MongoDBService mongoDB)
        {
            _mongoDB = mongoDB;
        }

        // Main admin page
        public async Task<IActionResult> Index()
        {
            var isAdmin = HttpContext.Session.GetString("Role") == "Admin";
            if (!isAdmin) return Redirect("/Account/Login");

            var productsCount = await _mongoDB.Flowers.CountDocumentsAsync(_ => true);
            var usersCount = await _mongoDB.Users.CountDocumentsAsync(_ => true);

            ViewBag.ProductsCount = productsCount;
            ViewBag.UsersCount = usersCount;
            return View();
        }

        // Products management
        public async Task<IActionResult> Products()
        {
            var isAdmin = HttpContext.Session.GetString("Role") == "Admin";
            if (!isAdmin) return Redirect("/Account/Login");

            var products = await _mongoDB.Flowers.Find(_ => true).ToListAsync();
            return View(products);
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(string name, decimal price, string category, int stock = 10)
        {
            var product = new Flowerdb
            {
                Name = name,
                Price = price,
                Category = category,
                Stock = stock,
                Status = "available",
                CreatedAt = DateTime.UtcNow
            };
            await _mongoDB.Flowers.InsertOneAsync(product);
            TempData["message"] = "✅ Product added!";
            return Redirect("/Admin/Products");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            await _mongoDB.Flowers.DeleteOneAsync(p => p.Id == id);
            TempData["message"] = "✅ Product deleted!";
            return Redirect("/Admin/Products");
        }

        // Users management
        public async Task<IActionResult> Users()
        {
            var isAdmin = HttpContext.Session.GetString("Role") == "Admin";
            if (!isAdmin) return Redirect("/Account/Login");

            var users = await _mongoDB.Users.Find(_ => true).ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            await _mongoDB.Users.DeleteOneAsync(u => u.Id == id);
            TempData["message"] = "✅ User deleted!";
            return Redirect("/Admin/Users");
        }
    }
}