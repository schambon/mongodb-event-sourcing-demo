using EventProcessor;
using EventProcessor.Models;
using EventProcessor.Services;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        //builder.Services.Configure<MongoDatabaseSettings>(builder.Configuration.GetSection("MongoDatabase"));
        var config = hostContext.Configuration;
        services.AddSingleton(config.GetSection("MongoDatabase").Get<MongoConnectionSettings>());
        services.AddSingleton(config.GetSection("EventStoreDatabase").Get<EventStoreDatabaseSettings>());
        services.AddSingleton(config.GetSection("ChangeTrackingDatabase").Get<ChangeTrackingDatabaseSettings>());
        services.AddSingleton(config.GetSection("WidgetStoreDatabase").Get<WidgetStoreDatabaseSettings>());
        services.AddSingleton(config.GetSection("ShelfStoreDatabase").Get<ShelfStoreDatabaseSettings>());
        services.AddSingleton<MongoDBService>();        
        services.AddHostedService<WidgetUpdateProcessor>();
        services.AddHostedService<ShelfUpdateProcessor>();
    })
    .Build();


await host.RunAsync();
