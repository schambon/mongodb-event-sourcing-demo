using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventProcessor.Models;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = null;

    [BsonElement("entityUuid")]
    public string EntityUuid { get; set; } = "";

    [BsonElement("ts")]
    public DateTime? Timestamp { get; set; } = null;

    [BsonElement("type")]
    public string? EventType { get; set; } = null;

    [BsonElement("details")]
    public BsonDocument EventDetails { get; set; } = new BsonDocument();
 }