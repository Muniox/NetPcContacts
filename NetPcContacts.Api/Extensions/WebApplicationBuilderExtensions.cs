using System.Collections;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using NetPcContacts.Api.Middlewares;
using System.Reflection;
using System.Threading.RateLimiting;

namespace NetPcContacts.Api.Extensions
{
    /// <summary>
    /// Extension methods do konfiguracji warstwy prezentacji (API).
    /// </summary>
    public static class WebApplicationBuilderExtensions
    {
        /// <summary>
        /// Dodaje i konfiguruje serwisy warstwy prezentacji.
        /// </summary>
        public static void AddPresentation(this WebApplicationBuilder builder)
        {
            // Authentication & Authorization
            builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
            builder.Services.AddAuthorization();

            // Controllers
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();

            // Swagger/OpenAPI z dokumentacją XML
            builder.Services.AddSwaggerGen(options => 
            {
                // Podstawowe informacje o API
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "NetPcContacts API",
                    Version = "v1",
                    Description = "REST API do zarządzania kontaktami. Umożliwia przeglądanie, tworzenie, edycję i usuwanie kontaktów.",
                    Contact = new OpenApiContact
                    {
                        Name = "Support",
                        Email = "support@netpccontacts.com"
                    }
                });

                // Włączenie komentarzy XML
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

                // Bearer Token Authentication w Swagger UI
                options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Wprowadź poprawny JWT token w formacie: Bearer {token}",
                    Name = "Authorization",
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Type = SecuritySchemeType.Http
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme()
                        {
                            Reference = new OpenApiReference()
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "BearerAuth"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Get allowed origins from appsettings.json
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            var defaultOrigin = new string[] { "http://localhost:4200" };
            
            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(allowedOrigins ?? defaultOrigin)
                    .AllowCredentials();
                });
            });

            // Rate Limiting
            builder.Services.AddRateLimiter(options =>
            {
                // Polityka globalna - Fixed Window (100 żądań/min)
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name 
                            ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                            ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Polityka dla endpointów autoryzacji (10 żądań/min)
                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 10,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                // Polityka dla Commands (POST/PUT/DELETE - 30 żądań/min)
                options.AddPolicy("commands", httpContext =>
                    RateLimitPartition.GetSlidingWindowLimiter(
                        partitionKey: httpContext.User.Identity?.Name 
                            ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                            ?? "anonymous",
                        factory: partition => new SlidingWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 30,
                            QueueLimit = 0,
                            Window = TimeSpan.FromMinutes(1),
                            SegmentsPerWindow = 6
                        }));

                // Polityka dla Queries (GET - 100 tokenów)
                options.AddPolicy("queries", httpContext =>
                    RateLimitPartition.GetTokenBucketLimiter(
                        partitionKey: httpContext.User.Identity?.Name 
                            ?? httpContext.Connection.RemoteIpAddress?.ToString() 
                            ?? "anonymous",
                        factory: partition => new TokenBucketRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            TokenLimit = 100,
                            QueueLimit = 0,
                            ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                            TokensPerPeriod = 50
                        }));

                // Obsługa błędów 429 Too Many Requests
                options.OnRejected = async (context, token) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    
                    if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                    {
                        await context.HttpContext.Response.WriteAsync(
                            $"Too many requests. Please try again after {retryAfter.TotalSeconds} seconds.", 
                            token);
                    }
                    else
                    {
                        await context.HttpContext.Response.WriteAsync(
                            "Too many requests. Please try again later.", 
                            token);
                    }
                };
            });

            // Middlewares
            builder.Services.AddScoped<ErrorHandlingMiddleware>();
        }
    }
}
