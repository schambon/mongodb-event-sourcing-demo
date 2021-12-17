namespace EventProcessor.Models;

public class WidgetStoreDatabaseSettings
{
    public string DatabaseName { get; set; } = "eventstore";

    public string CollectionName { get; set; } = "events";

}