using WordCloudService.Services;

namespace WordCloudService;

public class Startup
{
    private readonly IConfiguration _config;
    public Startup(IConfiguration config) => _config = config;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        
        services.AddHttpClient<IWordCloudeService, WordCloudeService>(client =>
        {
            var baseUrl = _config.GetValue<string>("QuickChart:BaseUrl")?.TrimEnd('/') 
                          ?? "https://quickchart.io";
            client.BaseAddress = new Uri(baseUrl + "/");
        });
    }

    public void Configure(WebApplication app, IHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.MapControllers();
    }
}
