using Microsoft.AspNetCore.Identity;
using MovieReservation.APi.Middleware;
using MovieReservation.Data.Entities.Identity;
using MovieReservation.Infrastructure.Data.Context;
using MovieReservation.Infrastructure.Data.DataSeed;
using MovieReservation.Infrastructure.Identity.Context;
using MovieReservation.Infrastructure.Identity.Seed;

namespace MovieReservation.APi.Extentions
{
    public static class ConfigureMiddleware
    {
        public async static Task<WebApplication> ConfigureMiddlewareAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider;
            var identityContext = service.GetRequiredService<AppIdentityDbContext>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            var appDbContext = service.GetRequiredService<AppDbContext>();
            var loggerFactory = service.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();

            try
            {
                await RoleSeeder.SeedAsync(roleManager);
                await UserSeeder.SeedAsync(userManager);

                // Seed application data from JSON files
                var seeder = new DataSeeder(appDbContext, loggerFactory.CreateLogger<DataSeeder>());

                // Get the correct seed data path
                // AppContext.BaseDirectory points to: bin\Debug\net9.0
                // We need to go to: MovieReservation.Infrastructure\Data\SeedData
                var seedDataPath = GetSeedDataPath();

                logger.LogInformation("Checking seed data path: {Path}", seedDataPath);

                if (Directory.Exists(seedDataPath))
                {
                    logger.LogInformation("Seed data directory found. Starting database seeding...");
                    await seeder.SeedAsync(seedDataPath);
                }
                else
                {
                    logger.LogWarning("Seed data path not found: {Path}", seedDataPath);
                    logger.LogWarning("Available files in current directory: {Files}",
                        string.Join(", ", Directory.GetFiles(AppContext.BaseDirectory).Select(Path.GetFileName)));
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
            app.UseHttpsRedirection();
            // ADD RESPONSE COMPRESSION (before rate limiting and CORS)
            app.UseResponseCompression();
            // ADD RATE LIMITING MIDDLEWARE
            app.UseMiddleware<RateLimitingMiddleware>();

            var corsPolicy = app.Environment.IsDevelopment() ? "AllowAll" : "AllowFrontend";
            app.UseCors(corsPolicy);
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapDefaultEndpoints();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Movie Reservation API v1");
                    options.RoutePrefix = string.Empty;
                }
                    );
                app.MapOpenApi();
            }

            app.MapControllers();

            return app;
        }

        /// <summary>
        /// Get the correct path to the seed data directory
        /// Handles both development and production scenarios
        /// </summary>
        private static string GetSeedDataPath()
        {
            // Method 1: Direct path from project root (most reliable)
            var projectRoot = GetProjectRoot();
            var seedDataPath = Path.Combine(projectRoot, "MovieReservation.Infrastructure", "Data", "SeedData");

            if (Directory.Exists(seedDataPath))
            {
                return seedDataPath;
            }

            // Method 2: Fallback - relative from bin directory
            // From: bin\Debug\net9.0 → MovieReservation.Infrastructure\Data\SeedData
            var basePath = AppContext.BaseDirectory;
            var fallbackPath = Path.Combine(basePath, "..", "..", "..", "MovieReservation.Infrastructure", "Data", "SeedData");
            fallbackPath = Path.GetFullPath(fallbackPath); // Normalize the path

            if (Directory.Exists(fallbackPath))
            {
                return fallbackPath;
            }

            // Method 3: Last resort - check common development locations
            var commonPaths = new[]
            {
                    Path.Combine(Directory.GetCurrentDirectory(), "MovieReservation.Infrastructure", "Data", "SeedData"),
                    Path.Combine(Directory.GetCurrentDirectory(), "..", "MovieReservation.Infrastructure", "Data", "SeedData"),
                    Path.Combine(Directory.GetCurrentDirectory(), "Data", "SeedData"),
                };

            foreach (var path in commonPaths)
            {
                if (Directory.Exists(path))
                {
                    return Path.GetFullPath(path);
                }
            }

            // If nothing found, return the most likely path (for better error messages)
            return fallbackPath;
        }

        /// <summary>
        /// Get the project root directory
        /// </summary>
        private static string GetProjectRoot()
        {
            var assemblyPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var projectRoot = Path.GetDirectoryName(assemblyPath);

            // Navigate up from bin\Debug\net9.0 to project root
            while (!string.IsNullOrEmpty(projectRoot) &&
                   !Directory.Exists(Path.Combine(projectRoot, "MovieReservation.Infrastructure")))
            {
                projectRoot = Directory.GetParent(projectRoot)?.FullName;
            }

            // If still not found, return parent of current directory
            if (string.IsNullOrEmpty(projectRoot) || !Directory.Exists(Path.Combine(projectRoot, "MovieReservation.Infrastructure")))
            {
                projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.FullName ?? "";
            }

            return projectRoot ?? "";
        }
    }
}
