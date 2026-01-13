using Microsoft.EntityFrameworkCore;
using MovieReservation.Infrastructure.Data.Context;
using MovieReservation.Infrastructure.Identity.Context;

namespace MovieReservation.APi.Extentions
{
    /// <summary>
    /// Database initialization service
    /// Applies pending migrations and seeds initial data
    /// </summary>
    public static class DatabaseInitializer
    {
        /// <summary>
        /// Initialize databases (apply migrations and seed data)
        /// </summary>
        public static async Task<WebApplication> InitializeDatabaseAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                logger.LogInformation("??? Starting database initialization...");

                // Initialize Identity Database
                await InitializeIdentityDatabaseAsync(services, logger);

                // Initialize Application Database
                await InitializeApplicationDatabaseAsync(services, logger);

                logger.LogInformation("? Database initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error during database initialization");
                // Don't throw - allow app to continue even if DB init fails
                // This allows for graceful handling of connection issues
            }

            return app;
        }

        /// <summary>
        /// Initialize Identity Database with migrations
        /// </summary>
        private static async Task InitializeIdentityDatabaseAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                logger.LogInformation("?? Initializing Identity Database...");

                var identityContext = services.GetRequiredService<AppIdentityDbContext>();

                // Check if database exists
                var canConnect = await identityContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogWarning("?? Cannot connect to Identity Database. Attempting to create...");
                    await identityContext.Database.EnsureCreatedAsync();
                    logger.LogInformation("?? Identity Database created");
                }

                // Get pending migrations
                var pendingMigrations = (await identityContext.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("?? Applying {MigrationCount} pending migrations to Identity Database", pendingMigrations.Count);
                    
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogInformation("  ? Applying migration: {MigrationName}", migration);
                    }

                    // Apply all pending migrations
                    await identityContext.Database.MigrateAsync();
                    logger.LogInformation("? All Identity Database migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("?? No pending migrations for Identity Database");
                }

                // Verify tables exist
                var tables = await GetTableNamesAsync(identityContext);
                logger.LogInformation("?? Identity Database tables: {TableCount} tables", tables.Count);
                foreach (var table in tables.Take(10))
                {
                    logger.LogDebug("  - {TableName}", table);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error initializing Identity Database");
                throw;
            }
        }

        /// <summary>
        /// Initialize Application Database with migrations
        /// </summary>
        private static async Task InitializeApplicationDatabaseAsync(IServiceProvider services, ILogger logger)
        {
            try
            {
                logger.LogInformation("?? Initializing Application Database...");

                var appContext = services.GetRequiredService<AppDbContext>();

                // Check if database exists
                var canConnect = await appContext.Database.CanConnectAsync();
                if (!canConnect)
                {
                    logger.LogWarning("?? Cannot connect to Application Database. Attempting to create...");
                    await appContext.Database.EnsureCreatedAsync();
                    logger.LogInformation("?? Application Database created");
                }

                // Get pending migrations
                var pendingMigrations = (await appContext.Database.GetPendingMigrationsAsync()).ToList();

                if (pendingMigrations.Any())
                {
                    logger.LogInformation("?? Applying {MigrationCount} pending migrations to Application Database", pendingMigrations.Count);
                    
                    foreach (var migration in pendingMigrations)
                    {
                        logger.LogInformation("  ? Applying migration: {MigrationName}", migration);
                    }

                    // Apply all pending migrations
                    await appContext.Database.MigrateAsync();
                    logger.LogInformation("? All Application Database migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("?? No pending migrations for Application Database");
                }

                // Verify tables exist
                var tables = await GetTableNamesAsync(appContext);
                logger.LogInformation("?? Application Database tables: {TableCount} tables", tables.Count);
                foreach (var table in tables.Take(10))
                {
                    logger.LogDebug("  - {TableName}", table);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "? Error initializing Application Database");
                throw;
            }
        }

        /// <summary>
        /// Get all table names from the database
        /// </summary>
        private static async Task<List<string>> GetTableNamesAsync(DbContext context)
        {
            var connection = context.Database.GetDbConnection();
            
            try
            {
                await connection.OpenAsync();
                using var command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT TABLE_NAME 
                    FROM INFORMATION_SCHEMA.TABLES 
                    WHERE TABLE_TYPE = 'BASE TABLE' 
                    ORDER BY TABLE_NAME";

                var tables = new List<string>();
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }

                return tables;
            }
            finally
            {
                connection.Close();
            }
        }
    }
}