using NetPcContacts.Api.Extensions;
using NetPcContacts.Application.Extensions;
using NetPcContacts.Domain.Entities;
using NetPcContacts.Infrastructure.Extensions;
using NetPcContacts.Infrastructure.Seeders;

namespace NetPcContacts.Api;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Added Presentation Layer
        builder.AddPresentation();

        // Added Application Layer
        builder.Services.AddApplication();
        
        // Added Infrastructure Layer
        builder.Services.AddInfrastructure(builder.Configuration);
        
        var app = builder.Build();

        // Seeders
        var scope = app.Services.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<IApplicationSeeder>();
        await seeder.Seed();

        // Exception handling
        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Mapowanie Identity API z Rate Limiting
        app.MapGroup("api/identity")
            .WithTags("Identity")
            .MapIdentityApi<User>()
            .RequireRateLimiting("auth");

        app.UseHttpsRedirection();

        app.UseCors("CorsPolicy");

        // WA�NE: UseRateLimiter PRZED Authentication i Authorization
        app.UseRateLimiter();

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}