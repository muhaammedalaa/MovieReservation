using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MovieReservation.Data.Contracts;
using MovieReservation.Data.Entities.Identity;
using MovieReservation.Data.Mapping.Category;
using MovieReservation.Data.Mapping.Movie;
using MovieReservation.Data.Mapping.Reservation;
using MovieReservation.Data.Mapping.Showtime;
using MovieReservation.Data.Service.Contract;
using MovieReservation.Infrastructure;
using MovieReservation.Infrastructure.Data.Context;
using MovieReservation.Infrastructure.Identity.Context;
using MovieReservation.Service.Models;
using MovieReservation.Service.Services.Auth;
using MovieReservation.Service.Services.Movie;
using MovieReservation.Service.Services.Payment;
using MovieReservation.Service.Services.Reservation;
using MovieReservation.Service.Services.Showtime;
using MovieReservation.Service.Services.Token;
using Stripe;
using System.Text;

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
            services.AddMappingServices(Configuration);
            services.AddApplicationServices();
            services.AddAuthenticationServices(Configuration);
            services.AddStripeServices(Configuration);
            services.AddCorsServices(Configuration);
            services.AddCompressionServices();
            services.AddEmailServices(Configuration);

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
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Movie Reservation API",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT token like this: Bearer {your token}"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
            });

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
        private static IServiceCollection AddMappingServices(this IServiceCollection services, IConfiguration Configuration)

        {
            services.AddAutoMapper(M => M.AddProfile(new MovieProfile()));
            services.AddAutoMapper(M => M.AddProfile(new CategoryProfile()));
            services.AddAutoMapper(M => M.AddProfile(new ShowtimeProfile()));
            services.AddAutoMapper(M => M.AddProfile(new ReservationProfile()));

            return services;
        }
        private static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IMovieService, MovieService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IReservationService, ReservationService>();
            services.AddScoped<IShowtimeService, ShowtimeService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPaymentService, PaymentService>();
            // Add other services here as you create them

            return services;
        }
        private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration Configuration)
        {
            var jwtSettings = Configuration.GetSection("Jwt");
            var secretKey = jwtSettings["SecretKey"];
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            if (string.IsNullOrEmpty(secretKey) || secretKey.Length < 32)
                throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateAudience = true,
                        ValidAudience = audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
            return services;


        }
        private static IServiceCollection AddStripeServices(this IServiceCollection services, IConfiguration Configuration)
        {
            var stripeSettings = Configuration.GetSection("Stripe");
            var secretKey = stripeSettings["SecretKey"];

            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Stripe SecretKey is not configured");

            StripeConfiguration.ApiKey = secretKey;

            return services;
        }
        private static IServiceCollection AddCorsServices(this IServiceCollection services, IConfiguration Configuration)
        {
            var corsSettings = Configuration.GetSection("Cors");
            var allowedOrigins = corsSettings.GetSection("AllowedOrigins").Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:5173" };
            var allowedMethods = corsSettings.GetSection("AllowedMethods").Get<string[]>()
                ?? new[] { "GET", "POST", "PUT", "DELETE", "OPTIONS" };
            var allowedHeaders = corsSettings.GetSection("AllowedHeaders").Get<string[]>()
                ?? new[] { "*" };
            services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", builder =>
                {
                    builder
                    .WithOrigins(allowedOrigins)
                    .WithMethods(allowedMethods)
                    .WithHeaders(allowedHeaders)
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition", "X-Pagination");
                });
                options.AddPolicy("AllowAll", builder =>
                builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()

                );
            });
            return services;
        }
        private static IServiceCollection AddCompressionServices(this IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = new[]
                {
                "text/plain",
                "text/css",
                "application/javascript",
                "application/json",
                "application/xml",
                "text/xml",
                "application/atom+xml",
                "image/svg+xml"
            };
            });
            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;
            });
            services.Configure<BrotliCompressionProviderOptions>(options =>
            {
                options.Level = System.IO.Compression.CompressionLevel.Optimal;

            });
            return services;

        }
        private static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration Configuration)

        {
            var emailSettings = Configuration.GetSection("EmailSettings");
            services.Configure<EmailSettings>(emailSettings);
            return services;
        }
    }
}
