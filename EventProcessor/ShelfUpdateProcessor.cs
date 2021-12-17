namespace EventProcessor;

using EventProcessor.Models;
using EventProcessor.Services;
using MongoDB.Driver;
using MongoDB.Bson;

public class ShelfUpdateProcessor : BackgroundService
{
    private readonly ILogger<ShelfUpdateProcessor> _logger;

    private readonly IMongoClient _mongoClient;

    private readonly IMongoCollection<Event> _eventCollection;
    private readonly IMongoCollection<BsonDocument> _changeTrackingCollection;
    private readonly IMongoCollection<BsonDocument> _shelfCollection;

    public ShelfUpdateProcessor(ILogger<ShelfUpdateProcessor> logger, 
                  MongoDBService mongoService,
                  EventStoreDatabaseSettings eventStoreSettings,
                  ChangeTrackingDatabaseSettings changeTrackingDatabaseSettings,
                  ShelfStoreDatabaseSettings shelfStoreDatabaseSettings)
    {   
        _logger = logger;
        _mongoClient = mongoService.MongoClient;

        _eventCollection = mongoService.MongoClient
                                .GetDatabase(eventStoreSettings.DatabaseName)
                                .GetCollection<Event>(eventStoreSettings.CollectionName);

        _changeTrackingCollection = mongoService.MongoClient
                                .GetDatabase(changeTrackingDatabaseSettings.DatabaseName)
                                .GetCollection<BsonDocument>(changeTrackingDatabaseSettings.CollectionName);
                     
        _shelfCollection = mongoService.MongoClient
                                .GetDatabase(shelfStoreDatabaseSettings.DatabaseName)
                                .GetCollection<BsonDocument>(shelfStoreDatabaseSettings.CollectionName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var tokenList = _changeTrackingCollection.Find(Builders<BsonDocument>.Filter.Eq("_id", "ShelfUpdateProcessor")).Limit(1).ToList();

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

                using(var session = _mongoClient.StartSession())
                {
                    session.StartTransaction();

                    Event evt = change.FullDocument;
                    var up = Builders<BsonDocument>.Update;
                    var ft = Builders<Event>.Filter;

                    switch(evt.EventType) {
                        case "Widget.New":
                            _shelfCollection.UpdateOne(
                                session,
                                new BsonDocument("_id", evt.EventDetails["shelf"]),
                                up.Inc("widgets", 1),
                                new UpdateOptions { IsUpsert = true }
                            );
                            break;
                        case "Widget.Move":
                            Event previous = _eventCollection.Find(session, ft.And(new[]{
                                ft.In("type", new[]{"Widget.New", "Widget.Move"}),
                                ft.Eq("entityUuid", evt.EntityUuid),
                                ft.Lt("ts", evt.Timestamp)
                            })).Sort(Builders<Event>.Sort.Descending("ts")).Limit(1).First();

                            _shelfCollection.UpdateOne(
                                session,
                                new BsonDocument("_id", previous.EventDetails["shelf"]),
                                up.Inc("widgets", -1)
                            );
                            _shelfCollection.UpdateOne(
                                session,
                                new BsonDocument("_id", evt.EventDetails["shelf"]),
                                up.Inc("widgets", 1),
                                new UpdateOptions { IsUpsert = true }
                            );
                            break;
                        case "Widget.Remove":
                            Event prev = _eventCollection.Find(session, ft.And(new[]{
                                ft.In("type", new[]{"Widget.New", "Widget.Move"}),
                                ft.Eq("entityUuid", evt.EntityUuid),
                                ft.Lt("ts", evt.Timestamp)
                            })).Sort(Builders<Event>.Sort.Descending("ts")).Limit(1).First();

                            _shelfCollection.UpdateOne(
                                session,
                                new BsonDocument("_id", prev.EventDetails["shelf"]),
                                up.Inc("widgets", -1)
                            );
                            break;
                    }
                    
                    _changeTrackingCollection.UpdateOne(
                        session,
                        new BsonDocument("_id", "ShelfUpdateProcessor"),
                        Builders<BsonDocument>.Update.Set("token", change.ResumeToken),
                        new UpdateOptions { IsUpsert = true }
                    );

                    session.CommitTransaction();
                }

            });
        }

    }
}
