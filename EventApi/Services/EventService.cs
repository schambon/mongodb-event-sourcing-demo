using EventApi.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace EventApi.Services;
public class EventService
{
    private readonly IMongoCollection<Event> _collection;

    public EventService(MongoDBService mongoService, IOptions<EventStoreDatabaseSettings> settings)
    {
        var mongoClient = mongoService.MongoClient;
        var db = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _collection = db.GetCollection<Event>(settings.Value.CollectionName);
    }

    public string CreateEvent(Event newEvent)
    {
        _collection.InsertOne(newEvent);
        return newEvent.Id;
    }
}