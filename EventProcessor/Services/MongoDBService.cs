
using EventProcessor.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace EventProcessor.Services;


public class MongoDBService
{
    public MongoClient MongoClient { get; }

    public MongoDBService(IOptions<EventProcessor.Models.MongoDatabaseSettings> settings)
    {
        MongoClient = new MongoClient(settings.Value.ConnectionString);
    }
}
