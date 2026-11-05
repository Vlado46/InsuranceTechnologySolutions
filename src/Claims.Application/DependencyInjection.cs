using Claims.Application.Interfaces;
using Claims.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Claims.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IClaimService, ClaimService>();
        services.AddScoped<ICoverService, CoverService>();

        return services;
    }
}
