
using EventApi.Models;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace EventApi.Services;


public class MongoDBService
{
    public MongoClient MongoClient { get; }

    public MongoDBService(IOptions<EventApi.Models.MongoDatabaseSettings> settings)
    {
        MongoClient = new MongoClient(settings.Value.ConnectionString);
    }
}
