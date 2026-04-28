using Microsoft.AspNetCore.Mvc;
using FlowerShop.Services;
using FlowerShop.Models;
using MongoDB.Driver;

namespace FlowerShop.Controllers
{
    public class SeedController : Controller
    {
        private readonly MongoDBService _mongoDB;

        public SeedController(MongoDBService mongoDB)
        {
            _mongoDB = mongoDB;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var flowerCount = await _mongoDB.Flowers.CountDocumentsAsync(_ => true);
            var userCount = await _mongoDB.Users.CountDocumentsAsync(_ => true);

            ViewBag.FlowerCount = flowerCount;
            ViewBag.UserCount = userCount;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SeedData()
        {
            try
            {
                // Clear existing data
                await _mongoDB.Flowers.DeleteManyAsync(_ => true);
                await _mongoDB.Users.DeleteManyAsync(_ => true);
                await _mongoDB.Sales.DeleteManyAsync(_ => true);

                // Seed Flowers
                var flowers = new List<Flowerdb>
                {
                    new Flowerdb
                    {
                        Name = "Red Roses",
                        Description = "Classic red roses for love and passion",
                        Price = 29.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1554224155-6726b3ff858f?w=400",
                        Category = "Roses",
                        Stock = 50,
                        Sold = 15
                    },
                    new Flowerdb
                    {
                        Name = "Pink Tulips",
                        Description = "Delicate pink tulips symbolizing affection",
                        Price = 24.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1587059393461-7bc8f6b2a3b9?w=400",
                        Category = "Tulips",
                        Stock = 30,
                        Sold = 8
                    },
                    new Flowerdb
                    {
                        Name = "Sunflowers",
                        Description = "Bright sunflowers for happiness and warmth",
                        Price = 19.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1518685046166-4fb28c62ed68?w=400",
                        Category = "Sunflowers",
                        Stock = 25,
                        Sold = 20
                    },
                    new Flowerdb
                    {
                        Name = "Lavender",
                        Description = "Fragrant lavender for relaxation",
                        Price = 22.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1587014611670-f64f41b8d318?w=400",
                        Category = "Herbals",
                        Stock = 40,
                        Sold = 5
                    },
                    new Flowerdb
                    {
                        Name = "White Lilies",
                        Description = "Elegant white lilies for purity",
                        Price = 34.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1594434005250-bf8c383a8ee4?w=400",
                        Category = "Lilies",
                        Stock = 20,
                        Sold = 12
                    },
                    new Flowerdb
                    {
                        Name = "Mixed Bouquet",
                        Description = "Beautiful mixed bouquet for any occasion",
                        Price = 49.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1548797439-2a595ab5b1e3?w=400",
                        Category = "Bouquets",
                        Stock = 15,
                        Sold = 3
                    }
                };

                await _mongoDB.Flowers.InsertManyAsync(flowers);

                // Seed Admin User
                var adminUser = new ApplicationUserMongo
                {
                    Username = "admin",
                    Email = "admin@blossomshop.com",
                    PasswordHash = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8", // Password: "admin123"
                    Role = "Admin"
                };
                await _mongoDB.Users.InsertOneAsync(adminUser);

                // Seed Sample Sales
                var sampleSales = new List<Sale>
                {
                    new Sale { FlowerId = flowers[0].Id, FlowerName = flowers[0].Name, /* ... */ }
                };
                await _mongoDB.Sales.InsertManyAsync(sampleSales);

                TempData["Success"] = "✅ Database seeded successfully! Admin: admin/admin123";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Error: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}