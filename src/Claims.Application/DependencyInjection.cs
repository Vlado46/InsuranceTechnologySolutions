using Claims.Application.Interfaces;
using Claims.Application.Services;
using Claims.Application.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddValidatorsFromAssemblyContaining<CreateClaimRequestValidator>();
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<ICoverService, CoverService>();

        return services;
    }
}
