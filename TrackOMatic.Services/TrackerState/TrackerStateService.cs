using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services.TrackerState;

namespace TrackOMatic.Services;

/// <summary>
/// Coordinator service for the six tracker-state domains (item tracking, barriers, hints, progression, spoiler, collectibles).
/// Provides high-level operations and aggregates state changes from individual domain services.
/// </summary>
public class TrackerStateService : ITrackerStateService
{
    public event EventHandler<TrackerStateChangedEventArgs>? StateChanged;

    private readonly IItemTrackingService _itemTracking;
    private readonly IBarrierService _barrier;
    // private readonly IHintPanelService _hints;
    private readonly IEndGameProgressionService _progression;
    private readonly ILevelOrderService _levelOrder;
    private readonly ISpoilerLogService _spoilerLog;
    private readonly ICollectiblesService _collectibles;
    private readonly IDataPersistenceService _persistence;
    private readonly ISavedProgressProvider _progressProvider;

    public TrackerStateService(
        IItemTrackingService itemTracking,
        IBarrierService barrier,
        // IHintPanelService hints,
        IEndGameProgressionService progression,
        ILevelOrderService levelOrder,
        ISpoilerLogService spoilerLog,
        ICollectiblesService collectibles,
        IDataPersistenceService persistence,
        ISavedProgressProvider progressProvider)
    {
        _itemTracking = itemTracking ?? throw new ArgumentNullException(nameof(itemTracking));
        _barrier = barrier ?? throw new ArgumentNullException(nameof(barrier));
        // _hints = hints ?? throw new ArgumentNullException(nameof(hints));
        _progression = progression ?? throw new ArgumentNullException(nameof(progression));
        _levelOrder = levelOrder ?? throw new ArgumentNullException(nameof(levelOrder));
        _spoilerLog = spoilerLog ?? throw new ArgumentNullException(nameof(spoilerLog));
        _collectibles = collectibles ?? throw new ArgumentNullException(nameof(collectibles));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _progressProvider = progressProvider ?? throw new ArgumentNullException(nameof(progressProvider));

        // Subscribe to domain service events to re-raise as aggregated state change
        _itemTracking.ItemStateChanged += (s, e) => OnTrackerStateChanged(nameof(IItemTrackingService.ItemStateChanged));
        _barrier.BlockerBarrierAmountChanged += (s, e) => OnTrackerStateChanged(nameof(IBarrierService.BlockerBarrierAmountChanged));
        _barrier.BlockerBarrierTypeChanged += (s, e) => OnTrackerStateChanged(nameof(IBarrierService.BlockerBarrierTypeChanged));
        _barrier.HelmBarrierAmountChanged += (s, e) => OnTrackerStateChanged(nameof(IBarrierService.HelmBarrierAmountChanged));
        _barrier.HelmBarrierTypeChanged += (s, e) => OnTrackerStateChanged(nameof(IBarrierService.HelmBarrierTypeChanged));
        // _hints.HintStateChanged += (s, e) => OnTrackerStateChanged(nameof(IHintPanelService.HintStateChanged));
        _progression.BlastStateChanged += (s, e) => OnTrackerStateChanged(nameof(IEndGameProgressionService.BlastStateChanged));
        _progression.GauntletStateChanged += (s, e) => OnTrackerStateChanged(nameof(IEndGameProgressionService.GauntletStateChanged));
        _levelOrder.LevelOrderChanged += (s, e) => OnTrackerStateChanged(nameof(ILevelOrderService.LevelOrderChanged));
        _spoilerLog.SpoilerLogStatusChanged += (s, e) => OnTrackerStateChanged(nameof(ISpoilerLogService.SpoilerLogStatusChanged));
        _collectibles.CollectibleChanged += (s, e) => OnTrackerStateChanged(nameof(ICollectiblesService.CollectibleChanged));
    }

    public SavedProgress GetCurrentState()
    {
        // Allow returning a fallback if the progress provider is not available
        return _progressProvider?.CurrentProgress ?? new();
    }

    public void LoadState(SavedProgress newState)
    {
        // TODO: Distribute SavedProgress to all services
        throw new NotImplementedException();
    }

    public void Reset()
    {
        // TODO: Reset all services
        throw new NotImplementedException();
    }

    private void OnTrackerStateChanged(string domainChanged)
    {
        var args = new TrackerStateChangedEventArgs
        {
            CurrentState = GetCurrentState(),
            DomainChanged = domainChanged,
            SpecificChange = null,
            ChangeReason = Logic.Enums.ChangeReason.DomainUpdate,
        }
        ;
        StateChanged?.Invoke(this, args);
    }
}
