using FlowerShop.Models;
using FlowerShop.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace FlowerShop.Controllers
{
    public class DebugController : Controller
    {
        private readonly MongoDBService _mongoDB;

        public DebugController(MongoDBService mongoDB)
        {
            _mongoDB = mongoDB;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _mongoDB.Users.Find(_ => true).ToListAsync();
            var admin = users.FirstOrDefault(u => u.Username == "admin");

            return View(new
            {
                UsersCount = users.Count,
                AdminExists = admin != null,
                AdminRole = admin?.Role ?? "NONE",
                AdminActive = admin?.IsActive ?? false,
                FirstUser = users.FirstOrDefault()?.Username ?? "NONE"
            });
        }
    }
}