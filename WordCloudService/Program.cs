using WordCloudService;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://+:15003");

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app, app.Environment);
app.Run();