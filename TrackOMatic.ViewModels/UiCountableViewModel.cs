using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;
using TrackOMatic.ViewModels.Broadcast;

namespace TrackOMatic.ViewModels;

public class UiCountableViewModel: CountableViewModel, INotifyPropertyChanged
{
    private readonly IUserSettingsService _userSettingsService;

    public UiCountableViewModel(
        ItemType itemType,
        ICollectiblesService collectiblesService,
        IParsedSpoilerDataService parsedSpoilerDataService,
        IUserSettingsService userSettingsService
    ) : base(itemType, collectiblesService, parsedSpoilerDataService)
    {
        _userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
    }

    public void IncrementCount()
    {
        if (!_userSettingsService.Autotracking)
        {
            ++Count;
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
