namespace EventApi.Models;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Shelf
{
    [BsonId]
    public int Id { get; set; } = -1;

    [BsonElement("widgets")]
    public int Widgets { get; set; } = 0;

}