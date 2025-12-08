using ActivityProvider.Endpoints;
using ActivityProvider.Factory;
using ActivityProvider.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<IActorProcessFactory, ActorProcessFactory>();
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapActivityProviderEndpoints();

app.Run();