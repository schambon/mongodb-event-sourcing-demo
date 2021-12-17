namespace EventApi.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonIgnoreExtraElements]
public class Widget
{
    [BsonId]
    public string Uuid { get; set; } = "";

    [BsonElement("status")]
    public string Status { get; set; } = "UNKNOWN";

    [BsonElement("description")]
    public string Description { get; set; } = "";

    [BsonElement("shelf")]
    public int? Shelf { get; set; }

    [BsonElement("location")]
    public string? Location { get; set; }

    [BsonElement("history")]
    public IEnumerable<WidgetHistory> History { get; set; }
}