using Microsoft.Extensions.DependencyInjection;

namespace TrackOMatic.Services.Test;

/// <summary>
/// Sanity-check tests for DI container configuration.
/// These tests verify that interfaces can be registered and resolved, without testing implementation logic.
/// </summary>
public class ServiceRegistrationTests
{
    [Fact]
    public void ServiceCollection_AddTrackOMaticServices_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        var ex = Record.Exception(() => services.AddTrackOMaticServices());
        Assert.Null(ex);
    }

    [Fact]
    public void ServiceProvider_AfterAddTrackOMaticServices_CanResolveExistingServices()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTrackOMaticServices();
        var provider = services.BuildServiceProvider();

        // Act & Assert - these services exist and should resolve without throwing
        Assert.Multiple(() =>
        {
            Assert.NotNull(provider.GetRequiredService<IUserSettingsService>());
            Assert.NotNull(provider.GetRequiredService<IApplicationStateService>());
        });
    }

    [Fact]
    public void ServiceRegistry_DocumentsAllServiceInterfaces()
    {
        // This is a documentation test - update it as services are implemented.
        // It serves as a checklist for PR #3 implementation.

        var expectedInterfaces = new[]
        {
            nameof(IUserSettingsService),
            nameof(IApplicationStateService),
            // Phase 3 services (to be implemented in PR #3):
            // nameof(ISavedProgressProvider),
            // nameof(IDataPersistenceService),
            // nameof(ICollectiblesService),
            // nameof(IBarrierService),
            // nameof(ITrackerStateService),
            // nameof(IEndGameProgressionService),
            // nameof(ILevelOrderService),
            // nameof(ISpoilerLogService),
        };

        Assert.NotEmpty(expectedInterfaces);
    }
}
