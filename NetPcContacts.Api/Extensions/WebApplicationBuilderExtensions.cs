using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using NetPcContacts.Api.Middlewares;

namespace NetPcContacts.Api.Extensions
{
    public static class WebApplicationBuilderExtensions
    {
        public static void AddPresentation(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication().AddBearerToken(IdentityConstants.BearerScheme);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen( options => 
            {
                options.AddSecurityDefinition("BearerAuth", new OpenApiSecurityScheme()
                {
                    In = ParameterLocation.Header,
                    Description = "Enter proper JWT token",
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
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost") // TODO: zmienić (port) po dodaniu frontendu
                    .AllowCredentials();
                });
            });

            // Middlewares
            builder.Services.AddScoped<ErrorHandlingMiddleware>();
        }
    }
}
