using MongoDB.Driver;
using FlowerShop.Models;
using MongoDB.Bson;

namespace FlowerShop.Services
{
    public class MongoDBService
    {
        private readonly IMongoDatabase _database;

        public MongoDBService(IConfiguration configuration)
        {
            var client = new MongoClient(configuration.GetConnectionString("MongoDB"));
            _database = client.GetDatabase("FlowerShop");
        }

        public IMongoCollection<Flowerdb> Flowers => _database.GetCollection<Flowerdb>("flowers");
        public IMongoCollection<ApplicationUserMongo> Users => _database.GetCollection<ApplicationUserMongo>("users");
        public IMongoCollection<Sale> Sales => _database.GetCollection<Sale>("sales");

        // Manager methods
        public async Task<List<Flowerdb>> GetLowStockFlowersAsync(int threshold = 10) =>
            await Flowers.Find(f => f.Stock < threshold).ToListAsync();

        public async Task UpdateFlowerAsync(string id, Flowerdb flower) =>
            await Flowers.ReplaceOneAsync(f => f.Id == id, flower);

        public async Task DeleteFlowerAsync(string id) =>
            await Flowers.DeleteOneAsync(f => f.Id == id);

        public async Task<List<ApplicationUserMongo>> GetAllUsersAsync() =>
            await Users.Find(_ => true).ToListAsync();

        public async Task DeleteUserAsync(string id) =>
            await Users.DeleteOneAsync(u => u.Id == id);

        public async Task UpdateUserAsync(string id, ApplicationUserMongo user) =>
            await Users.ReplaceOneAsync(u => u.Id == id, user);
    }
}