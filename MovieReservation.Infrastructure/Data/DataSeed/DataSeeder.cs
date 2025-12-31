using Microsoft.Extensions.Logging;
using MovieReservation.Data.Entities;
using MovieReservation.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MovieReservation.Infrastructure.Data.DataSeed
{
    public class DataSeeder
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger<DataSeeder> _logger;

        public DataSeeder(AppDbContext dbContext, ILogger<DataSeeder> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }
        public async Task SeedAsync(string seedDataPath)
        {
            try
            {
                _logger.LogInformation("Starting database seeding process...");
                // Seed categories first (no dependencies)
                await SeedCategoriesAsync(seedDataPath);

                // Seed theaters (no dependencies)
                await SeedTheatersAsync(seedDataPath);

                // Seed movies (depends on categories)
                await SeedMoviesAsync(seedDataPath);

                // Seed showtimes (depends on movies and theaters)
                await SeedShowtimesAsync(seedDataPath);

                _logger.LogInformation("Database seeding completed successfully!");


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during database seeding.");
                throw;
            }
        }
        // Seed categories from JSON file
        private async Task SeedCategoriesAsync(string seedDataPath)
        {
            if (_dbContext.Categories.Any())
            {
                _logger.LogInformation("Categories already exist. Skipping seed.");
                return;
            }
            try
            {
                var categoriesPath = Path.Combine(seedDataPath, "categories.json");
                if (!File.Exists(categoriesPath))
                {
                    _logger.LogWarning("Categories seed file not found at {Path}", categoriesPath);
                    return;
                }
                var json = await File.ReadAllTextAsync(categoriesPath);
                var categories = JsonSerializer.Deserialize<List<Category>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (categories?.Any() == true)
                {
                    await _dbContext.Categories.AddRangeAsync(categories);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Seeded {Count} categories", categories.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding categories");
                throw;
            }
        }
        // Seed theaters from JSON file

        private async Task SeedTheatersAsync(string seedDataPath)
        {

            if (_dbContext.Theaters.Any())
            {
                _logger.LogInformation("Theaters already exist. Skipping seed.");
                return;
            }

            try
            {
                var theatersPath = Path.Combine(seedDataPath, "theaters.json");
                if (!File.Exists(theatersPath))
                {
                    _logger.LogWarning("Theaters seed file not found at {Path}", theatersPath);
                    return;
                }

                var json = await File.ReadAllTextAsync(theatersPath);
                var theaters = JsonSerializer.Deserialize<List<Theater>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (theaters?.Any() == true)
                {
                    await _dbContext.Theaters.AddRangeAsync(theaters);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Seeded {Count} theaters", theaters.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding theaters");
                throw;
            }
        }
        // Seed movies from JSON file
        private async Task SeedMoviesAsync(string seedDataPath)
        {
            if (_dbContext.Movies.Any())
            {
                _logger.LogInformation("Movies already exist. Skipping seed.");
                return;
            }

            try
            {
                var moviesPath = Path.Combine(seedDataPath, "movies.json");
                if (!File.Exists(moviesPath))
                {
                    _logger.LogWarning("Movies seed file not found at {Path}", moviesPath);
                    return;
                }

                var json = await File.ReadAllTextAsync(moviesPath);
                var movies = JsonSerializer.Deserialize<List<Movie>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (movies?.Any() == true)
                {
                    // Fix datetime initialization
                    foreach (var movie in movies)
                    {
                        movie.CreatedAt = DateTime.UtcNow;
                    }

                    await _dbContext.Movies.AddRangeAsync(movies);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Seeded {Count} movies", movies.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding movies");
                throw;
            }
        }
        // Seed showtimes from JSON file
        private async Task SeedShowtimesAsync(string seedDataPath)
        {
            if (_dbContext.Showtimes.Any())
            {
                _logger.LogInformation("Showtimes already exist. Skipping seed.");
                return;
            }

            try
            {
                var showtimesPath = Path.Combine(seedDataPath, "showtimes.json");
                if (!File.Exists(showtimesPath))
                {
                    _logger.LogWarning("Showtimes seed file not found at {Path}", showtimesPath);
                    return;
                }

                var json = await File.ReadAllTextAsync(showtimesPath);
                var showtimes = JsonSerializer.Deserialize<List<Showtime>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (showtimes?.Any() == true)
                {
                    await _dbContext.Showtimes.AddRangeAsync(showtimes);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Seeded {Count} showtimes", showtimes.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error seeding showtimes");
                throw;
            }
        }

    }
}
