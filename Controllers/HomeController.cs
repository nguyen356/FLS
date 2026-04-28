using Microsoft.AspNetCore.Mvc;
using FlowerShop.Services;
using FlowerShop.Models;
using MongoDB.Driver;

namespace FlowerShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly MongoDBService _mongoDB;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HomeController(MongoDBService mongoDB, IHttpContextAccessor httpContextAccessor)
        {
            _mongoDB = mongoDB;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index()
        {
            var flowers = await _mongoDB.Flowers.Find(Builders<Flowerdb>.Filter.Empty).Limit(6).ToListAsync();
            ViewBag.Flowers = flowers;
            ViewBag.Username = HttpContext.Session.GetString("Username");
            ViewBag.Role = HttpContext.Session.GetString("Role");
            return View();
        }

        public async Task<IActionResult> Shop()
        {
            var flowers = await _mongoDB.Flowers.Find(Builders<Flowerdb>.Filter.Empty).ToListAsync();
            return View(flowers);
        }

        public async Task<IActionResult> FlowerDetail(string id)
        {
            var flower = await _mongoDB.Flowers.Find(f => f.Id == id).FirstOrDefaultAsync();
            return View(flower);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View(new ContactModel());

        [HttpPost]
        public IActionResult Contact(ContactModel model)
        {
            if (ModelState.IsValid)
            {
                TempData["Success"] = "Thank you! We'll reply soon.";
                return RedirectToAction("Contact");
            }
            return View(model);
        }
    }
}