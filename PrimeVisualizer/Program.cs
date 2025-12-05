using PrimeVisualizer.Hubs;
using PrimeVisualizer.Services;
using PrimeVisualizer.Services.Algorithms;

namespace PrimeVisualizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSignalR();

            // CORS
            builder.Services.AddCors(option =>
            {
                option.AddDefaultPolicy(policy =>
                {
                    policy.WithOrigins("http://localhost:3000", "http://localhost:5050  ")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
                });
            });

            // Add Algorithms
            builder.Services.AddScoped<IPrimeAlgorithm, EratosthenesSieve>();
            builder.Services.AddScoped<IPrimeAlgorithm, TrialDivisionOptimized>();
            builder.Services.AddScoped<IPrimeAlgorithm, TrialDivision>();
            builder.Services.AddScoped<IPrimeAlgorithm, TrialDivisionWithPrecomputedPrimes>();

            builder.Services.AddScoped<RaceService>();

            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseCors();
            app.UseStaticFiles();
            app.UseRouting();

            // Mapping Hubs
            app.MapHub<RaceHub>("/RaceHub");

            app.MapControllers();

            app.Run();
        }
    }
}
