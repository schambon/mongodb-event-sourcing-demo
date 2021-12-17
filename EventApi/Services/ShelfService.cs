using EventApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace EventApi.Services;

public class ShelfService
{
    private readonly IMongoCollection<Shelf> _collection;
    private readonly ILogger<ShelfService> _logger;

    public ShelfService(MongoDBService mongoService, IOptions<ShelfStoreDatabaseSettings> settings, ILogger<ShelfService> logger)
    {
        var mongoClient = mongoService.MongoClient;
        var db = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = db.GetCollection<Shelf>(settings.Value.CollectionName);
        _logger = logger;
    }

    public IEnumerable<Shelf> All()
    {
        return _collection.Find(new BsonDocument()).ToEnumerable();
    }

    public Shelf FindById(int id) {
        _logger.LogInformation("Fetching shelf id = " + id);
        return _collection.Find(Builders<Shelf>.Filter.Eq("_id", id)).First();
    }

    public int TotalWidgets() {
        var x = _collection.Aggregate()
            .Group(x => 1, g => new { Key = g.Key, TotalWidgets = g.Sum(s => s.Widgets)})
            .ToList();
        return x[0].TotalWidgets;

    }
}