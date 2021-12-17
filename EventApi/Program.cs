using EventApi.Models;
using EventApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<MongoDatabaseSettings>(builder.Configuration.GetSection("MongoDatabase"));
builder.Services.Configure<EventStoreDatabaseSettings>(
    builder.Configuration.GetSection("EventStoreDatabase")
);
builder.Services.Configure<WidgetStoreDatabaseSettings>(
    builder.Configuration.GetSection("WidgetStoreDatabase")
);
builder.Services.Configure<ShelfStoreDatabaseSettings>(
    builder.Configuration.GetSection("ShelfStoreDatabase")
);
builder.Services.AddSingleton<MongoDBService>();
builder.Services.AddSingleton<EventService>();
builder.Services.AddSingleton<WidgetService>();
builder.Services.AddSingleton<ShelfService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
