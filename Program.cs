using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Amazon;
using BlazorAIChatBot_with_AWS_Bedrock.Components;
using BlazorAIChatBotOpenAI.Components;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using Amazon.BedrockRuntime.Model;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<ILogger>(static serviceProvider =>
{
    var lf = serviceProvider.GetRequiredService<ILoggerFactory>();
    return lf.CreateLogger(typeof(Program));
});

// Register the chat client for AWS-Bedrock
builder.Services.AddSingleton<AmazonBedrockRuntimeClient>(static serviceProvider =>
{

var credentials = new BasicAWSCredentials("", "");
var config = new AmazonBedrockRuntimeConfig { RegionEndpoint = RegionEndpoint.USEast1 };
var client = new AmazonBedrockRuntimeClient(credentials, config);

    return client;
});

// Register default chat messages
builder.Services.AddSingleton<List<Message>>(static serviceProvider =>
{
    return new List<Message>()
    {
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
