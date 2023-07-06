

using MongoDB.Driver;
using EventProcessor.Models;

namespace EventProcessor.Services;


public class MongoDBService
{
    private readonly ILogger<MongoDBService> _logger;

    public MongoClient MongoClient { get; }

    public MongoDBService(ILogger<MongoDBService> logger, MongoConnectionSettings settings)
    {
        _logger = logger;
        MongoClient = new MongoClient(settings.ConnectionString);
        _logger.LogInformation("Connected to {}", settings.ConnectionString);
    }
}
