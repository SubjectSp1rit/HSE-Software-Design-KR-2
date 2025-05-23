using Microsoft.EntityFrameworkCore;
using FileAnalysisService.Application.Services;
using FileAnalysisService.Infrastructure.Persistence;
using FileAnalysisService.Infrastructure.Adapters;

public class Startup
{
    private readonly IConfiguration _config;
    public Startup(IConfiguration config) => _config = config;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddCors(opts =>
        {
            opts.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });
        
        services.AddDbContext<AnalysisDbContext>(opts =>
            opts.UseNpgsql(_config.GetConnectionString("DefaultConnection")));
        
        services.AddHttpClient<IFileStorerAdapter, FileStorerHttpAdapter>(c =>
        {
            c.BaseAddress = new Uri(_config["FileStorer:BaseUrl"]!);
        });
        
        services.AddHttpClient<IWordCloudAdapter, WordCloudHttpAdapter>(c =>
        {
            c.BaseAddress = new Uri(_config["WordCloud:BaseUrl"]!);
        });
        
        services.AddScoped<IFileAnalyseService, FileAnalyseService>();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    public void Configure(WebApplication app, IWebHostEnvironment env)
    {
        app.UseCors("AllowAll");
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();
    }
}