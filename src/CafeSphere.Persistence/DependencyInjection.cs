using CafeSphere.Domain.Repositories;
using CafeSphere.Persistence.Configurations;
using CafeSphere.Persistence.Context;
using CafeSphere.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CafeSphere.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(
            configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoDbContext, MongoDbContext>();
        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        return services;
    }
}
