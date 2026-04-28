using FlowerShop.Models;
using FlowerShop.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FlowerShop.Controllers
{
    public class ManagerController : Controller
    {
        private readonly MongoDBService _mongoDB;

        public ManagerController(MongoDBService mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [HttpGet]
        public async Task<IActionResult> Inventory()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin") return Unauthorized();

            var flowers = await _mongoDB.Flowers.Find(_ => true).SortBy(f => f.Name).ToListAsync();
            var lowStock = await _mongoDB.GetLowStockFlowersAsync();

            ViewBag.LowStock = lowStock;
            ViewBag.Flowers = flowers;
            return View();
        }

        [HttpGet]
        public IActionResult AddFlower()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin") return Unauthorized();
            return View(new Flowerdb());
        }

        [HttpPost]
        public async Task<IActionResult> AddFlower(Flowerdb flower)
        {
            if (!ModelState.IsValid) return View(flower);

            flower.Id = ObjectId.GenerateNewId().ToString();
            flower.CreatedAt = DateTime.UtcNow;

            await _mongoDB.Flowers.InsertOneAsync(flower);
            TempData["Success"] = $"✅ {flower.Name} added successfully!";
            return RedirectToAction("Inventory");
        }

        [HttpGet]
        public async Task<IActionResult> EditFlower(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin") return Unauthorized();

            var flower = await _mongoDB.Flowers.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (flower == null) return NotFound();
            return View(flower);
        }

        [HttpPost]
        public async Task<IActionResult> EditFlower(Flowerdb flower)
        {
            if (!ModelState.IsValid) return View(flower);

            await _mongoDB.UpdateFlowerAsync(flower.Id, flower);
            TempData["Success"] = $"✅ {flower.Name} updated successfully!";
            return RedirectToAction("Inventory");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteFlower(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin") return Unauthorized();

            var flower = await _mongoDB.Flowers.Find(f => f.Id == id).FirstOrDefaultAsync();
            if (flower != null)
            {
                await _mongoDB.DeleteFlowerAsync(id);
                TempData["Success"] = $"✅ {flower.Name} deleted!";
            }
            return RedirectToAction("Inventory");
        }

        [HttpGet]
        public async Task<IActionResult> Customers()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin") return Unauthorized();

            var users = await _mongoDB.GetAllUsersAsync();
            ViewBag.Users = users;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCustomer(string id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "Manager" && role != "Admin") return Unauthorized();

            await _mongoDB.DeleteUserAsync(id);
            TempData["Success"] = "✅ Customer deleted!";
            return RedirectToAction("Customers");
        }
    }
}