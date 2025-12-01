using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using NetPcContacts.Application.Behaviors;

namespace NetPcContacts.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static void AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ServiceCollectionExtensions).Assembly;
        
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });
        
        // Rejestracja pipeline behavior dla walidacji
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        services.AddValidatorsFromAssembly(applicationAssembly);
    }
}