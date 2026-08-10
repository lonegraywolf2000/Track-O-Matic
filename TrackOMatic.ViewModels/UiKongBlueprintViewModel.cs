using System.ComponentModel;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;
using TrackOMatic.ViewModels.Broadcast;

namespace TrackOMatic.ViewModels;

public class UiKongBlueprintViewModel: KongBlueprintViewModel, INotifyPropertyChanged
{
    private readonly IUserSettingsService _userSettingsService;

    public UiKongBlueprintViewModel(
        ItemType collectedType,
        ItemType turnedInType,
        ICollectiblesService collectiblesService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        ISavedProgressProvider savedProgressProvider,
        IUserSettingsService userSettingsService
    ) : base(collectedType, turnedInType, collectiblesService, parsedSpoilerDataService, savedProgressProvider)
    {
        _userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
    }

    public void IncrementCount()
    {
        if (!_userSettingsService.Autotracking)
        {
            Count = int.Min(Count + 1, 8);
        }
    }

    public void DecrementCount()
    {
        if (!_userSettingsService.Autotracking)
        {
            Count = int.Max(Count - 1, 0);
        }
    }
}
