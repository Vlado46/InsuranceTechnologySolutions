using Claims.Application.Interfaces;
using Claims.Infrastructure.Auditing;
using Claims.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Claims.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Connection strings are read inside these lambdas so that integration tests can supply their own.
        services.AddDbContext<AuditContext>(options =>
            options.UseSqlServer(GetConnectionString(configuration, "Audit")));

        services.AddSingleton<IMongoClient>(_ => new MongoClient(GetConnectionString(configuration, "Mongo")));
        services.AddDbContext<ClaimsContext>((provider, options) =>
            options.UseMongoDB(provider.GetRequiredService<IMongoClient>(), configuration["MongoDb:DatabaseName"]!));

        services.AddScoped<IClaimRepository, ClaimRepository>();
        services.AddScoped<ICoverRepository, CoverRepository>();
        services.AddScoped<IAuditer, Auditer>();

        return services;
    }

    private static string GetConnectionString(IConfiguration configuration, string name) =>
        configuration.GetConnectionString(name)
            ?? throw new InvalidOperationException($"Connection string '{name}' is not configured.");
}
