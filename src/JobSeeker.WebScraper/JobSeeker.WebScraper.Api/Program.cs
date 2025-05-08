using JobSeeker.WebScraper.Api.Configuration;
using JobSeeker.WebScraper.Application.Configuration;
using JobSeeker.WebScraper.MessageBroker.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureLogging();
builder.ConfigureMessageBroker();
builder.ConfigureApplication();

var app = builder.Build();
app.Run();