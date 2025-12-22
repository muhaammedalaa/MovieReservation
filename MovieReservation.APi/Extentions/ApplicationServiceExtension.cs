using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MovieReservation.Data.Entities.Identity;
using MovieReservation.Infrastructure.Data.Context;
using MovieReservation.Infrastructure.Identity.Context;
using System;

namespace MovieReservation.APi.Extentions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddServiceDefaults(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddBuiltInServices();
            services.AddBuiltInServicesSwagger();
            services.AddDbContextServices(Configuration);
            services.AddIdentityServices();

            return services;
        }
        private static IServiceCollection AddBuiltInServices(this IServiceCollection services)
        {
            services.AddControllers();


            return services;
        }
        private static IServiceCollection AddBuiltInServicesSwagger(this IServiceCollection services)
        {

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddOpenApi();
            return services;
        }
        private static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration Configuration)
        {
            services.AddDbContext<AppDbContext>(options =>

            options.UseSqlServer(
                Configuration.GetConnectionString("DefaultConnections"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));
            services.AddDbContext<AppIdentityDbContext>(options
                => options.UseSqlServer(
                Configuration.GetConnectionString("IdentityConnections"),
                b => b.MigrationsAssembly(typeof(AppIdentityDbContext).Assembly.FullName)));

            return services;
        }
        private static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {

            services.AddIdentity<AppUser, IdentityRole>()
                  .AddEntityFrameworkStores<AppIdentityDbContext>();
            return services;
        }
    }
}
