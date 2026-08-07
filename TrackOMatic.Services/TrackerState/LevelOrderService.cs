using System;
using System.Collections.Generic;
using System.Text;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Services.TrackerState;

/// <summary>
/// A service for managing which region belongs to which level in the tracker state.
/// This also wraps access to the <see cref="SavedProgress"/> state in case it changes via reset or load.
/// </summary>
public class LevelOrderService: ILevelOrderService
{
    private readonly ISavedProgressProvider _progressProvider;
    private SavedProgress _savedProgress;

    public event EventHandler<LevelOrderStateChangedEventArgs>? LevelOrderChanged;

    public LevelOrderService(ISavedProgressProvider progressProvider)
    {
        _progressProvider = progressProvider ?? throw new ArgumentNullException(nameof(progressProvider));
        _savedProgress = _progressProvider.CurrentProgress;
        // Set up listener for SavedProgress changes
        _progressProvider.ProgressChanged += OnProgressChanged;
    }

    private void InitializeCollections()
    {
        _savedProgress.LevelOrder ??= new(8);
    }

    private void OnProgressChanged(object? sender, ProgressReplacedEventArgs e)
    {
        _savedProgress = e.NewProgress;
        InitializeCollections();
    }

    #region ILevelOrderService Implementation

    public IList<int> GetLevelOrder()
    {
        var list = _savedProgress.LevelOrder;
        if (list is null || list.Count < 7)
        {
            return [0, 0, 0, 0, 0, 0, 0, 0];
        }
        return _savedProgress.LevelOrder;
    }

    public void SetLevelOrder(IEnumerable<int> levelOrder)
    {
        var previous = GetLevelOrder();
        var newList = levelOrder.ToList();
        _savedProgress.LevelOrder = newList;
        LevelOrderChanged?.Invoke(this, new LevelOrderStateChangedEventArgs([.. previous], newList, ChangeReason.UserModified));
    }

    #endregion
}
