namespace EventProcessor;

using EventProcessor.Models;
using EventProcessor.Services;
using MongoDB.Driver;
using MongoDB.Bson;

public class WidgetUpdateProcessor : BackgroundService
{
    private readonly ILogger<WidgetUpdateProcessor> _logger;

    private readonly IMongoClient _mongoClient;

    private readonly IMongoCollection<Event> _eventCollection;
    private readonly IMongoCollection<BsonDocument> _changeTrackingCollection;
    private readonly IMongoCollection<BsonDocument> _widgetCollection;

    public WidgetUpdateProcessor(ILogger<WidgetUpdateProcessor> logger, 
                  MongoDBService mongoService,
                  EventStoreDatabaseSettings eventStoreSettings,
                  ChangeTrackingDatabaseSettings changeTrackingDatabaseSettings,
                  WidgetStoreDatabaseSettings widgetStoreDatabaseSettings)
    {   
        _logger = logger;
        _mongoClient = mongoService.MongoClient;

        _eventCollection = mongoService.MongoClient
                                .GetDatabase(eventStoreSettings.DatabaseName)
                                .GetCollection<Event>(eventStoreSettings.CollectionName);

        try
        {
            mongoService.MongoClient
                                .GetDatabase(changeTrackingDatabaseSettings.DatabaseName)
                                .CreateCollection(changeTrackingDatabaseSettings.CollectionName);
        }
        catch(MongoCommandException)
        {
            _logger.LogDebug("Cannot create change tracking collection, likely because it already exists");
            // this simply means the change tracking collection already exists. It's fine.
            // note we want the collection to exist before entering change processing, otherwise 
            // transactions may fail if both change stream processors try to create the collection
            // at the same time in a transaction
        }
        _changeTrackingCollection = mongoService.MongoClient
                                .GetDatabase(changeTrackingDatabaseSettings.DatabaseName)
                                .GetCollection<BsonDocument>(changeTrackingDatabaseSettings.CollectionName);
                                
        _widgetCollection = mongoService.MongoClient
                                .GetDatabase(widgetStoreDatabaseSettings.DatabaseName)
                                .GetCollection<BsonDocument>(widgetStoreDatabaseSettings.CollectionName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tokenList = _changeTrackingCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", "WidgetUpdateProcessor")).Limit(1).ToList();

        var csOptions = new ChangeStreamOptions();
        if (tokenList.Count == 1)
        {
            csOptions.ResumeAfter = (BsonDocument)tokenList[0]["token"];
        }
        var pipeline = 
            new EmptyPipelineDefinition<ChangeStreamDocument<Event>>()
            .Match(x => x.OperationType == ChangeStreamOperationType.Insert);

        using (var cursor = await _eventCollection.WatchAsync(pipeline, csOptions)) {
            await cursor.ForEachAsync(change => {
                _logger.LogInformation("Got an event: {}", change.ToString());

                Event evt = change.FullDocument;

                using (var session = _mongoClient.StartSession()) 
                {
                    session.StartTransaction();

                    switch(evt.EventType) {
                        case "Widget.New":
                            _widgetCollection.InsertOne(
                                session,
                                new BsonDocument {
                                    { "_id", evt.EntityUuid },
                                    { "status", "INSTORE"},
                                    { "description", evt.EventDetails["description"]},
                                    { "shelf", ((int)evt.EventDetails["shelf"])},
                                    { "history", new BsonArray
                                        {
                                            new BsonDocument {
                                                {"ts", evt.Timestamp},
                                                {"event", "New"},
                                                {"shelf", evt.EventDetails["shelf"]}
                                            }
                                        }
                                    }
                                }
                            );
                            break;
                        case "Widget.Move":
                            _widgetCollection.UpdateOne(
                                session,
                                new BsonDocument("_id", evt.EntityUuid),
                                Builders<BsonDocument>.Update.Combine(new[] {
                                    Builders<BsonDocument>.Update.Set("shelf", evt.EventDetails["shelf"]),
                                    Builders<BsonDocument>.Update.Push("history", 
                                        new BsonDocument {
                                            {"ts", evt.Timestamp},
                                            {"event", "Move"},
                                            {"shelf", evt.EventDetails["shelf"]}
                                        }
                                    )
                                })
                            );
                            break;
                        case "Widget.Remove":
                            var updateBuilder = Builders<BsonDocument>.Update;
                            _widgetCollection.UpdateOne(
                                session,
                                new BsonDocument("_id", evt.EntityUuid),
                                updateBuilder.Combine(new [] {
                                    updateBuilder.Set("status", "REMOVED"),
                                    updateBuilder.Set("location", evt.EventDetails["destination"]),
                                    updateBuilder.Unset("shelf"),
                                    updateBuilder.Push("history",
                                        new BsonDocument {
                                            {"ts", evt.Timestamp},
                                            {"event", "Remove"}
                                        }
                                    )
                                })
                            );
                            break;
                    }

                    _changeTrackingCollection.UpdateOne(
                        session,
                        new BsonDocument("_id", "WidgetUpdateProcessor"),
                        Builders<BsonDocument>.Update.Set("token", change.ResumeToken),
                        new UpdateOptions { IsUpsert = true }
                    );

                    session.CommitTransaction();
                }
                
            });
        }
    }
}
