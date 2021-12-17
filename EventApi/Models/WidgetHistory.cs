namespace EventApi.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class WidgetHistory
{
    [BsonElement("ts")]
    public DateTime ts { get; set; } = DateTime.Now;

    [BsonElement("event")]
    public string evtType { get; set; } = "UNKNOWN";

    [BsonElement("shelf")]
    public int? shelf { get; set; } = null;
}