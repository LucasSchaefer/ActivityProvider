using ActivityProvider.Endpoints;
using ActivityProvider.Factory;
using ActivityProvider.Services;
using ActivityProvider.Services.Memento;
using ActivityProvider.Services.Proxy;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition =
        JsonIgnoreCondition.WhenWritingNull;
});

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProcessService>();
builder.Services.AddScoped<TranslationManager>();
builder.Services.AddScoped<IProcessService>(p => 
    new ProcessProxyService(p.GetRequiredService<ProcessService>(), p.GetRequiredService<TranslationManager>())
);
builder.Services.AddScoped<IActorProcessFactory, ActorProcessFactory>();
// Add services to the container.

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

app.MapActivityProviderEndpoints();

app.Run();