using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Services.TrackerState;

namespace TrackOMatic.Services.Test.TrackerState;

/// <summary>
/// Unit tests for ItemTrackingService.
/// Tests item tracking state management, caching, and event firing.
/// </summary>
public class ItemTrackingServiceTests
{
    private readonly SavedProgress _progress;
    private readonly TestSavedProgressProvider _provider;
    private readonly ItemTrackingService _sut;

    public ItemTrackingServiceTests()
    {
        _progress = new SavedProgress();
        _provider = new TestSavedProgressProvider(_progress);
        _sut = new ItemTrackingService(_provider);
    }

    #region GetItemState Tests

    [Fact]
    public void GetItemState_WithExistingItem_ReturnsItem()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        // Act
        var retrieved = _sut.GetItemState(ItemName.DONKEY);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemName.DONKEY, retrieved.ItemName);
            Assert.Equal(RegionName.JUNGLE_JAPES, retrieved.Region);
        });
    }

    [Fact]
    public void GetItemState_WithNonexistentItem_ReturnsNull()
    {
        // Act
        var retrieved = _sut.GetItemState(ItemName.DIDDY);

        // Assert
        Assert.Null(retrieved);
    }

    #endregion

    #region SetItemState Tests

    [Fact]
    public void SetItemState_AddsNewItem()
    {
        // Arrange
        var newItem = new SavedItem(
            ItemName.DIDDY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );

        // Act
        _sut.SetItemState(ItemName.DIDDY, newItem);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DIDDY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemName.DIDDY, retrieved.ItemName);
            Assert.Equal(RegionName.JUNGLE_JAPES, retrieved.Region);
        });
    }

    [Fact]
    public void SetItemState_UpdatesExistingItem()
    {
        // Arrange
        var originalItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, originalItem);

        var updatedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.ANGRY_AZTEC,
            ItemVisibilityState.Hidden,
            true,
            0.5,
            false
        );

        // Act
        _sut.SetItemState(ItemName.DONKEY, updatedItem);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DONKEY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemName.DONKEY, retrieved.ItemName);
            Assert.Equal(RegionName.ANGRY_AZTEC, retrieved.Region);
            Assert.Equal(ItemVisibilityState.Hidden, retrieved.Starred);
            Assert.True(retrieved.Autotracked);
            Assert.Equal(0.5, retrieved.Opacity);
        });
    }

    [Fact]
    public void SetItemState_WithMismatchedItemName_ThrowsArgumentException()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            _sut.SetItemState(ItemName.DIDDY, item)
        );
    }

    [Fact]
    public void SetItemState_RaisesEvent()
    {
        // Arrange
        var eventFired = false;
        ItemStateChangedEventArgs? eventArgs = null;

        _sut.ItemStateChanged += (sender, args) =>
        {
            eventFired = true;
            eventArgs = args;
        };

        var newItem = new SavedItem(
            ItemName.DIDDY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );

        // Act
        _sut.SetItemState(ItemName.DIDDY, newItem);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.NotNull(eventArgs.UpdatedItem);
            Assert.Null(eventArgs.PreviousState);
        });
    }

    [Fact]
    public void SetItemState_RaisesEventWithPreviousState_WhenUpdating()
    {
        // Arrange
        var originalItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, originalItem);

        var eventArgs = (ItemStateChangedEventArgs?)null;
        _sut.ItemStateChanged += (sender, args) => eventArgs = args;

        var updatedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.ANGRY_AZTEC,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );

        // Act
        _sut.SetItemState(ItemName.DONKEY, updatedItem);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(eventArgs);
            Assert.NotNull(eventArgs.UpdatedItem);
            Assert.Equal(RegionName.ANGRY_AZTEC, eventArgs.UpdatedItem.Region);
            Assert.NotNull(eventArgs.PreviousState);
            Assert.Equal(RegionName.JUNGLE_JAPES, eventArgs.PreviousState.Region);
        });
    }

    #endregion

    #region ClearItemState Tests

    [Fact]
    public void ClearItemState_RemovesItem()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        // Act
        _sut.ClearItemState(ItemName.DONKEY);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DONKEY);
        Assert.Null(retrieved);
    }

    [Fact]
    public void ClearItemState_WithNonexistentItem_DoesNotThrow()
    {
        // Act & Assert - should not throw
        _sut.ClearItemState(ItemName.DIDDY);
    }

    [Fact]
    public void ClearItemState_RaisesEvent()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        var eventFired = false;
        _sut.ItemStateChanged += (sender, args) => eventFired = true;

        // Act
        _sut.ClearItemState(ItemName.DONKEY);

        // Assert
        Assert.True(eventFired);
    }

    #endregion

    #region GetItemsInRegion Tests

    [Fact]
    public void GetItemsInRegion_ReturnsItemsInRegion()
    {
        // Arrange
        var item1 = new SavedItem(ItemName.DONKEY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        var item2 = new SavedItem(ItemName.DIDDY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        _sut.SetItemState(ItemName.DONKEY, item1);
        _sut.SetItemState(ItemName.DIDDY, item2);

        // Act
        var itemsInJungle = _sut.GetItemsInRegion(RegionName.JUNGLE_JAPES).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(2, itemsInJungle.Count);
            Assert.All(itemsInJungle, item => Assert.Equal(RegionName.JUNGLE_JAPES, item.Region));
        });
    }

    [Fact]
    public void GetItemsInRegion_WithNoItemsInRegion_ReturnsEmpty()
    {
        // Act
        var itemsInAztec = _sut.GetItemsInRegion(RegionName.ANGRY_AZTEC).ToList();

        // Assert
        Assert.Empty(itemsInAztec);
    }

    #endregion

    #region GetItemsByVisibility Tests

    [Fact]
    public void GetItemsByVisibility_ReturnsItemsWithVisibility()
    {
        // Arrange
        var item1 = new SavedItem(ItemName.DONKEY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        var item2 = new SavedItem(ItemName.DIDDY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        _sut.SetItemState(ItemName.DONKEY, item1);
        _sut.SetItemState(ItemName.DIDDY, item2);

        // Act
        var visibleItems = _sut.GetItemsByVisibility(ItemVisibilityState.Visible).ToList();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(2, visibleItems.Count);
            Assert.All(visibleItems, item => Assert.Equal(ItemVisibilityState.Visible, item.Starred));
        });
    }

    [Fact]
    public void GetItemsByVisibility_WithNoItemsWithVisibility_ReturnsEmpty()
    {
        // Act
        var hiddenItems = _sut.GetItemsByVisibility(ItemVisibilityState.Hidden).ToList();

        // Assert
        Assert.Empty(hiddenItems);
    }

    #endregion

    #region Progress Replacement Tests

    [Fact]
    public void OnProgressChanged_ReinitializesCache()
    {
        // Arrange
        var newProgress = new SavedProgress();
        var newItem = new SavedItem(ItemName.LANKY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        newProgress.SavedItems[ItemName.LANKY] = newItem;

        // Act
        _provider.UpdateProgress(newProgress);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.LANKY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemName.LANKY, retrieved.ItemName);
            Assert.Equal(RegionName.JUNGLE_JAPES, retrieved.Region);
        });
    }


    #endregion

    #region ToggleStar Tests

    [Fact]
    public void ToggleStar_WithVisibleItem_TogglesHidden()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        // Act
        _sut.ToggleStar(ItemName.DONKEY);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DONKEY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemVisibilityState.Hidden, retrieved.Starred);
        });
    }

    [Fact]
    public void ToggleStar_WithHiddenItem_TogglesVisible()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Hidden,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        // Act
        _sut.ToggleStar(ItemName.DONKEY);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DONKEY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemVisibilityState.Visible, retrieved.Starred);
        });
    }

    [Fact]
    public void ToggleStar_WithNonexistentItem_CreatesEmptyAndTogglesVisible()
    {
        // Act
        _sut.ToggleStar(ItemName.DIDDY);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DIDDY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemName.DIDDY, retrieved.ItemName);
            Assert.Equal(RegionName.UNKNOWN, retrieved.Region);
            Assert.Equal(ItemVisibilityState.Visible, retrieved.Starred);
            Assert.False(retrieved.Autotracked);
            Assert.Equal(1.0, retrieved.Opacity);
            Assert.False(retrieved.Hinted);
        });
    }

    [Fact]
    public void ToggleStar_PreservesOtherProperties()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            true,
            0.75,
            true
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        // Act
        _sut.ToggleStar(ItemName.DONKEY);

        // Assert
        var retrieved = _sut.GetItemState(ItemName.DONKEY);
        Assert.Multiple(() =>
        {
            Assert.NotNull(retrieved);
            Assert.Equal(ItemName.DONKEY, retrieved.ItemName);
            Assert.Equal(RegionName.JUNGLE_JAPES, retrieved.Region);
            Assert.Equal(ItemVisibilityState.Hidden, retrieved.Starred);
            Assert.True(retrieved.Autotracked);
            Assert.Equal(0.75, retrieved.Opacity);
            Assert.True(retrieved.Hinted);
        });
    }

    [Fact]
    public void ToggleStar_RaisesEvent()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        var eventFired = false;
        ItemStateChangedEventArgs? eventArgs = null;
        _sut.ItemStateChanged += (sender, args) =>
        {
            eventFired = true;
            eventArgs = args;
        };

        // Act
        _sut.ToggleStar(ItemName.DONKEY);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(eventFired);
            Assert.NotNull(eventArgs);
            Assert.NotNull(eventArgs.UpdatedItem);
            Assert.Equal(ItemVisibilityState.Hidden, eventArgs.UpdatedItem.Starred);
            Assert.NotNull(eventArgs.PreviousState);
            Assert.Equal(ItemVisibilityState.Visible, eventArgs.PreviousState.Starred);
        });
    }

    [Fact]
    public void ToggleStar_WithBatchUpdate_DefersEvent()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        var eventCount = 0;
        _sut.ItemStateChanged += (sender, args) => eventCount++;

        // Act
        using (var batch = _sut.BeginBatchUpdate())
        {
            _sut.ToggleStar(ItemName.DONKEY);
            Assert.Equal(0, eventCount); // No event yet
        }

        // Assert
        Assert.Equal(1, eventCount);
    }

    [Fact]
    public void ToggleStar_MultipleToggles_AlternatesState()
    {
        // Arrange
        var item = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            false
        );
        _sut.SetItemState(ItemName.DONKEY, item);

        // Act & Assert - First toggle
        _sut.ToggleStar(ItemName.DONKEY);
        var afterFirstToggle = _sut.GetItemState(ItemName.DONKEY);
        Assert.Equal(ItemVisibilityState.Hidden, afterFirstToggle!.Starred);

        // Act & Assert - Second toggle
        _sut.ToggleStar(ItemName.DONKEY);
        var afterSecondToggle = _sut.GetItemState(ItemName.DONKEY);
        Assert.Equal(ItemVisibilityState.Visible, afterSecondToggle!.Starred);

        // Act & Assert - Third toggle
        _sut.ToggleStar(ItemName.DONKEY);
        var afterThirdToggle = _sut.GetItemState(ItemName.DONKEY);
        Assert.Equal(ItemVisibilityState.Hidden, afterThirdToggle!.Starred);
    }

    #endregion

    #region Batch Update Tests

    [Fact]
    public void BeginBatchUpdate_MultipleSetItemState_DeferRedUntilDispose()
    {
        // Arrange
        var item1 = new SavedItem(ItemName.DONKEY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        var item2 = new SavedItem(ItemName.DIDDY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        var eventCount = 0;
        _sut.ItemStateChanged += (sender, args) => eventCount++;

        // Act
        using (var batch = _sut.BeginBatchUpdate())
        {
            _sut.SetItemState(ItemName.DONKEY, item1);
            Assert.Equal(0, eventCount);
            _sut.SetItemState(ItemName.DIDDY, item2);
            Assert.Equal(0, eventCount);
        }

        // Assert
        Assert.Equal(2, eventCount);
    }

    [Fact]
    public void BeginBatchUpdate_WithClearedItem_FiresDeferredEvent()
    {
        // Arrange
        var item = new SavedItem(ItemName.DONKEY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0, false);
        _sut.SetItemState(ItemName.DONKEY, item);
        var eventCount = 0;
        _sut.ItemStateChanged += (sender, args) => eventCount++;

        // Act
        using (var batch = _sut.BeginBatchUpdate())
        {
            _sut.ClearItemState(ItemName.DONKEY);
            Assert.Equal(0, eventCount); // No event yet
        }

        // Assert
        Assert.Equal(1, eventCount);
        Assert.Null(_sut.GetItemState(ItemName.DONKEY));
    }

    #endregion
}
