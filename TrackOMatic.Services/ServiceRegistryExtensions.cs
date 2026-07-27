using Microsoft.Extensions.DependencyInjection;

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
        // Existing application services
        services.AddSingleton<IUserSettingsService, UserSettingsService>();
        services.AddSingleton<IApplicationStateService, ApplicationStateService>();

        // Phase 3 domain services (to be implemented in PR #3)
        // Interfaces are defined in TrackOMatic.Services.TrackerState but implementations are stubbed for now.
        // This will be populated once concrete implementations are ready.

        // TODO: Add service registrations in PR #3
        // services.AddSingleton<ISavedProgressProvider, SavedProgressProvider>();
        // services.AddSingleton<IDataPersistenceService, DataPersistenceService>();
        // services.AddSingleton<ICollectiblesService, CollectiblesService>();
        // services.AddSingleton<IBarrierService, BarrierService>();
        // services.AddSingleton<ITrackerStateService, TrackerStateService>();
        // services.AddSingleton<IEndGameProgressionService, EndGameProgressionService>();
        // services.AddSingleton<ILevelOrderService, LevelOrderService>();
        // services.AddSingleton<ISpoilerLogService, SpoilerLogService>();

        return services;
    }
}
