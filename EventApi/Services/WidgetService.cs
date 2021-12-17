using EventApi.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace EventApi.Services;

public class WidgetService
{
    private readonly IMongoCollection<Widget> _collection;
    private readonly ILogger<WidgetService> _logger;

    public WidgetService(MongoDBService mongoService, IOptions<WidgetStoreDatabaseSettings> settings, ILogger<WidgetService> logger)
    {
        var mongoClient = mongoService.MongoClient;
        var db = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = db.GetCollection<Widget>(settings.Value.CollectionName);
        _logger = logger;
    }

    internal IEnumerable<Widget> All()
    {
        return _collection.Find<Widget>(new BsonDocument()).ToEnumerable();
    }

    public Widget FindById(string uuid) {
        _logger.LogInformation("Fetching uuid = " + uuid);
        return _collection.Find(Builders<Widget>.Filter.Eq("_id", uuid)).First();
    }
}