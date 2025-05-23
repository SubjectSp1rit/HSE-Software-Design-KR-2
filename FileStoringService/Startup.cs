using Microsoft.EntityFrameworkCore;
using FileStoringService.Infrastructure.Persistence;
using FileStoringService.Infrastructure.Repositories;
using FileStoringService.Application.Services;

public class Startup
{
    private readonly IConfiguration _config;
    public Startup(IConfiguration config) => _config = config;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });
        
        services.AddDbContext<FileDbContext>(opts =>
            opts.UseNpgsql(_config.GetConnectionString("DefaultConnection")));
        
        services.AddScoped<IFileRepository, FileRepository>();
        services.AddScoped<IFileStorageService, FileStorageService>();
        
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