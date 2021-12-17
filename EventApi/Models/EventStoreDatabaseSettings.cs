namespace EventApi.Models;

public class EventStoreDatabaseSettings
{
    public string DatabaseName { get; set; } = "eventstore";

    public string CollectionName { get; set; } = "events";

}