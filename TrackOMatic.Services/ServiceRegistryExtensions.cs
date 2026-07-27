using Microsoft.Extensions.DependencyInjection;

using TrackOMatic.Services.SpoilerDeserialization;
using TrackOMatic.Services.TrackerState;

namespace TrackOMatic.Services;

/// <summary>
/// Extension methods for registering Track-O-Matic services in the dependency injection container.
/// </summary>
public static class ServiceRegistryExtensions
{
    /// <summary>
    /// Registers all application services (singletons and dependencies).
    /// </summary>
    public static IServiceCollection AddTrackOMaticServices(this IServiceCollection services)
    {
        // Spoiler deserialization services
        services.AddSingleton<IRawSpoilerFileDeserializer, RawSpoilerFileDeserializer>();

        // Existing application services
        services.AddSingleton<IUserSettingsService, UserSettingsService>();
        services.AddSingleton<IApplicationStateService, ApplicationStateService>();

        // TODO: Flesh out spoiler log service, track state service, and eventual hint service.
        services.AddSingleton<ISavedProgressProvider, SavedProgressProvider>();
        services.AddSingleton<IDataPersistenceService, DataPersistenceService>();
        services.AddSingleton<ICollectiblesService, CollectiblesService>();
        services.AddSingleton<IBarrierService, BarrierService>();
        services.AddSingleton<ITrackerStateService, TrackerStateService>();
        services.AddSingleton<IEndGameProgressionService, EndGameProgressionService>();
        services.AddSingleton<ILevelOrderService, LevelOrderService>();
        services.AddSingleton<ISpoilerLogService, SpoilerLogService>();

        return services;
    }
}
