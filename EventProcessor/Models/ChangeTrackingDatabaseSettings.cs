namespace EventProcessor.Models;

public class ChangeTrackingDatabaseSettings
{
    public string DatabaseName { get; set; } = "eventstore";

    public string CollectionName { get; set; } = "changetracking";
}