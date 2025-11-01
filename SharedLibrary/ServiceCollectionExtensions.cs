using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCacheSupport(this IServiceCollection services, IConfiguration configuration, string instanceName)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = new ConfigurationOptions
            {
                EndPoints = { configuration["Redis:Endpoint"]! },
                Password = configuration["Redis:Password"],
                AbortOnConnectFail = false
            };

            // prefix for cache keys
            options.InstanceName = instanceName;
        });

        return services;
    }
}