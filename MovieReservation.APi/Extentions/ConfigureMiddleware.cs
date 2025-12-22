using Microsoft.AspNetCore.Identity;
using MovieReservation.Data.Entities.Identity;
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
            var loggerfactory = service.GetRequiredService<ILoggerFactory>();
            try
            {
                await RoleSeeder.SeedAsync(roleManager);
                await UserSeeder.SeedAsync(userManager);
               
            }
            catch (Exception ex)
            {
                var logger = loggerfactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred seeding the DB.");
            }
            app.MapDefaultEndpoints();
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.MapOpenApi();
            }
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();
            return app;

        }
    }
}
