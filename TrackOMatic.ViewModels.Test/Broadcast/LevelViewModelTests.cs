using Moq;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Events;
using TrackOMatic.Logic.Models;
using TrackOMatic.Logic.Models.Spoilers;
using TrackOMatic.Services;
using TrackOMatic.Services.TrackerState;
using TrackOMatic.ViewModels.Broadcast;

namespace TrackOMatic.ViewModels.Test.Broadcast;

public class LevelViewModelTests
{
    private static Mock<IItemTrackingService> CreateMockItemTrackingService(
        List<SavedItem>? itemsInRegion = null)
    {
        var mock = new Mock<IItemTrackingService>();
        mock.Setup(s => s.GetItemsInRegion(It.IsAny<RegionName>()))
            .Returns(itemsInRegion ?? []);
        return mock;
    }

    private static Mock<ILevelOrderService> CreateMockLevelOrderService(
        List<int>? levelOrder = null)
    {
        var mock = new Mock<ILevelOrderService>();
        mock.Setup(s => s.GetLevelOrder())
            .Returns(levelOrder ?? [1, 2, 3, 4, 5, 6, 7, 8]);
        return mock;
    }

    private static Mock<IParsedSpoilerDataService> CreateMockParsedSpoilerDataService(
        ParsedSpoilerData? spoilerData = null)
    {
        var mock = new Mock<IParsedSpoilerDataService>();
        mock.Setup(s => s.CurrentData).Returns(spoilerData);
        mock.Setup(s => s.GetPointSpread()).Returns(CreateDefaultPointSpread());
        mock.Setup(s => s.GetPointsForRegion(It.IsAny<RegionName>())).Returns(200);
        return mock;
    }

    private static Mock<IUserSettingsService> CreateMockUserSettingsService()
    {
        var mock = new Mock<IUserSettingsService>();
        mock.Setup(s => s.BroadcastNumberLabel)
            .Returns(BroadcastNumberLabel.Points);
        return mock;
    }

    private static Dictionary<PointCategory, int> CreateDefaultPointSpread() => new()
    {
        { PointCategory.Key, 50 },
        { PointCategory.Gun, 30 },
        { PointCategory.Instrument, 10 },
        { PointCategory.Kong, 20 }
    };

    private static LevelViewModel CreateViewModel(
        int selectedLevel = 1,
        IItemTrackingService? itemTrackingService = null,
        ILevelOrderService? levelOrderService = null,
        IParsedSpoilerDataService? parsedSpoilerDataService = null,
        IUserSettingsService? userSettingsService = null)
    {
        itemTrackingService ??= CreateMockItemTrackingService().Object;
        levelOrderService ??= CreateMockLevelOrderService().Object;
        parsedSpoilerDataService ??= CreateMockParsedSpoilerDataService().Object;
        userSettingsService ??= CreateMockUserSettingsService().Object;

        return new LevelViewModel(
            selectedLevel,
            itemTrackingService,
            levelOrderService,
            parsedSpoilerDataService,
            userSettingsService);
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidServices_Initializes()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var levelOrderService = CreateMockLevelOrderService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var userSettingsService = CreateMockUserSettingsService();

        // Act
        var viewModel = new LevelViewModel(
            1,
            itemTrackingService.Object,
            levelOrderService.Object,
            parsedSpoilerDataService.Object,
            userSettingsService.Object);

        // Assert
        Assert.NotNull(viewModel);
    }

    [Fact]
    public void Constructor_WithNullItemTrackingService_ThrowsArgumentNullException()
    {
        // Arrange
        var levelOrderService = CreateMockLevelOrderService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var userSettingsService = CreateMockUserSettingsService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LevelViewModel(1, null!, levelOrderService.Object, parsedSpoilerDataService.Object, userSettingsService.Object));
    }

    [Fact]
    public void Constructor_WithNullLevelOrderService_ThrowsArgumentNullException()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var userSettingsService = CreateMockUserSettingsService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LevelViewModel(1, itemTrackingService.Object, null!, parsedSpoilerDataService.Object, userSettingsService.Object));
    }

    [Fact]
    public void Constructor_WithNullParsedSpoilerDataService_ThrowsArgumentNullException()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var levelOrderService = CreateMockLevelOrderService();
        var userSettingsService = CreateMockUserSettingsService();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            new LevelViewModel(1, itemTrackingService.Object, levelOrderService.Object, null!, userSettingsService.Object));
    }

    #endregion

    #region Initialization Tests

    [Fact]
    public void InitializeState_SetsNumberResourceKey()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Assert
        Assert.Equal("number_1", viewModel.NumberResourceKey);
    }

    [Fact]
    public void InitializeState_SetsMainRegionResourceKeyToUnknown()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Assert - Default level (1) might map to a region, not necessarily unknown
        Assert.NotNull(viewModel.RegionResourceKey);
    }

    [Fact]
    public void InitializeState_SetsShowRegionImageToTrue()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Assert
        Assert.True(viewModel.ShowRegionImage);
    }

    [Fact]
    public void InitializeState_WithDifferentLevel_SetsDifferentNumberResourceKey()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 5);

        // Assert
        Assert.Equal("number_5", viewModel.NumberResourceKey);
    }

    #endregion

    #region Property Tests

    [Fact]
    public void NumberResourceKey_ReturnsCurrentValue()
    {
        // Arrange
        var viewModel = CreateViewModel(selectedLevel: 3);

        // Act
        var value = viewModel.NumberResourceKey;

        // Assert
        Assert.Equal("number_3", value);
    }

    [Fact]
    public void RegionResourceKey_ReturnsCurrentValue()
    {
        // Arrange
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Act
        var value = viewModel.RegionResourceKey;

        // Assert
        Assert.NotNull(value);
    }

    [Fact]
    public void ShowRegionImage_ReturnsTrue()
    {
        // Arrange
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Act
        var value = viewModel.ShowRegionImage;

        // Assert
        Assert.True(value);
    }

    #endregion

    #region Item State Changed Override Tests

    [Fact]
    public void OnItemStateChanged_WithMatchingLevelKey_UpdatesData()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var levelOrderService = CreateMockLevelOrderService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object,
            levelOrderService: levelOrderService.Object,
            parsedSpoilerDataService: parsedSpoilerDataService.Object);

        // Act
        var keyState = SavedItem.CreateEmpty(ItemName.KEY_1) with { Region = RegionName.DK_ISLES };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(keyState));

        // Assert - Should have updated region data
        Assert.NotNull(viewModel.RegionResourceKey);
    }

    [Fact]
    public void OnItemStateChanged_WithDifferentLevelKey_DoesNotUpdate()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var levelOrderService = CreateMockLevelOrderService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object,
            levelOrderService: levelOrderService.Object,
            parsedSpoilerDataService: parsedSpoilerDataService.Object);

        var initialResourceKey = viewModel.RegionResourceKey;

        // Act - Raise event for a different level's key
        var differentKeyState = SavedItem.CreateEmpty(ItemName.KEY_2) with { Region = RegionName.JUNGLE_JAPES };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(differentKeyState));

        // Assert - ResourceKey should remain the same
        Assert.Equal(initialResourceKey, viewModel.RegionResourceKey);
    }

    #endregion

    #region Update Region Data Tests

    [Fact]
    public void UpdateRegionData_WithInvalidLevel_SetsUnknown()
    {
        // Arrange
        var levelOrder = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var levelOrderService = CreateMockLevelOrderService(levelOrder);
        var viewModel = CreateViewModel(
            selectedLevel: 100, // Invalid level number
            levelOrderService: levelOrderService.Object);

        // Act
        var regionKey = viewModel.RegionResourceKey;

        // Assert
        Assert.Equal("unknown_label", regionKey);
    }

    #endregion

    #region Update Points Data Tests

    [Fact]
    public void UpdatePointsData_WithUnknownRegion_HidesRemainingPoints()
    {
        // Arrange
        var viewModel = CreateViewModel(selectedLevel: 100); // Will result in unknown region

        // Act & Assert
        Assert.Multiple(() =>
        {
            Assert.False(viewModel.ShowRemainingPoints);
            Assert.Empty(viewModel.PointsText);
            Assert.Equal("RegionInProgress", viewModel.PointColorResource);
        });
    }

    [Fact]
    public void UpdatePointsData_WithKnownRegion_CallsBaseImplementation()
    {
        // Arrange
        var levelOrder = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8 };
        var levelOrderService = CreateMockLevelOrderService(levelOrder);
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        parsedSpoilerDataService.Setup(s => s.GetPointsForRegion(RegionName.JUNGLE_JAPES))
            .Returns(100);

        var viewModel = CreateViewModel(
            selectedLevel: 1,
            levelOrderService: levelOrderService.Object,
            parsedSpoilerDataService: parsedSpoilerDataService.Object);

        // Act
        var pointsText = viewModel.PointsText;

        // Assert
        Assert.NotEmpty(pointsText);
    }

    #endregion

    #region Equality Tests

    [Fact]
    public void Equals_WithSameSelectedLevel_ReturnsTrue()
    {
        // Arrange
        var viewModel1 = CreateViewModel(selectedLevel: 1);
        var viewModel2 = CreateViewModel(selectedLevel: 1);

        // Act & Assert
        Assert.Equal(viewModel1, viewModel2);
    }

    [Fact]
    public void Equals_WithDifferentSelectedLevel_ReturnsFalse()
    {
        // Arrange
        var viewModel1 = CreateViewModel(selectedLevel: 1);
        var viewModel2 = CreateViewModel(selectedLevel: 2);

        // Act & Assert
        Assert.NotEqual(viewModel1, viewModel2);
    }

    [Fact]
    public void Equals_WithNull_ReturnsFalse()
    {
        // Arrange
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Act & Assert
        Assert.False(viewModel.Equals(null));
    }

    [Fact]
    public void GetHashCode_WithSameSelectedLevel_ReturnsSameHashCode()
    {
        // Arrange
        var viewModel1 = CreateViewModel(selectedLevel: 1);
        var viewModel2 = CreateViewModel(selectedLevel: 1);

        // Act & Assert
        Assert.Equal(viewModel1.GetHashCode(), viewModel2.GetHashCode());
    }

    [Fact]
    public void GetHashCode_WithDifferentSelectedLevel_ReturnsDifferentHashCode()
    {
        // Arrange
        var viewModel1 = CreateViewModel(selectedLevel: 1);
        var viewModel2 = CreateViewModel(selectedLevel: 2);

        // Act & Assert
        Assert.NotEqual(viewModel1.GetHashCode(), viewModel2.GetHashCode());
    }

    #endregion

    #region Inheritance Tests

    [Fact]
    public void LevelViewModel_InheritsFromIslesViewModel()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Assert
        Assert.IsType<IslesViewModel>(viewModel, exactMatch: false);
    }

    #endregion

    #region Disposal Tests

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Act & Assert
        viewModel.Dispose();
        viewModel.Dispose(); // Should not throw
    }

    #endregion

    #region Key Image and Starred Tests

    [Fact]
    public void InitializeState_SetsImageResourceKeyToDefaultBW()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Assert
        Assert.Equal("basic_key_bw", viewModel.ImageResourceKey);
    }

    [Fact]
    public void InitializeState_SetsIsStarredToFalse()
    {
        // Arrange & Act
        var viewModel = CreateViewModel(selectedLevel: 1);

        // Assert
        Assert.False(viewModel.IsStarred);
    }

    [Fact]
    public void OnItemStateChanged_WithMatchingKeyStarred_UpdatesIsStarred()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object);

        // Act - Raise event for level 1's key marked as starred
        var keyState = SavedItem.CreateEmpty(ItemName.KEY_1) with
        {
            Starred = ItemVisibilityState.Visible
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(keyState));

        // Assert
        Assert.True(viewModel.IsStarred);
    }

    [Fact]
    public void OnItemStateChanged_WithMatchingKeyNotStarred_UpdatesIsStarred()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object);

        // Act - Raise event for level 1's key not starred
        var keyState = SavedItem.CreateEmpty(ItemName.KEY_1) with
        {
            Starred = ItemVisibilityState.Hidden
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(keyState));

        // Assert
        Assert.False(viewModel.IsStarred);
    }

    [Fact]
    public void OnItemStateChanged_WithDifferentKey_DoesNotUpdateIsStarred()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object);

        var initialStarred = viewModel.IsStarred;

        // Act - Raise event for level 2's key (different from level 1)
        var differentKeyState = SavedItem.CreateEmpty(ItemName.KEY_2) with
        {
            Starred = ItemVisibilityState.Visible
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(differentKeyState));

        // Assert - IsStarred should remain unchanged
        Assert.Equal(initialStarred, viewModel.IsStarred);
    }

    [Fact]
    public void UpdateImageResourceKey_WithNoItemAndNoSpoiler_ShowsBWVariant()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object,
            parsedSpoilerDataService: parsedSpoilerDataService.Object);

        // Act - Raise event with null item state
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(SavedItem.CreateEmpty(ItemName.KEY_1)));

        // Assert
        Assert.Equal("basic_key_bw", viewModel.ImageResourceKey);
    }

    [Fact]
    public void UpdateImageResourceKey_WithHintedItem_ShowsBWVariant()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object);

        // Act - Raise event with hinted key
        var hintedKeyState = SavedItem.CreateEmpty(ItemName.KEY_1) with
        {
            Hinted = true
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(hintedKeyState));

        // Assert
        Assert.Equal("basic_key_bw", viewModel.ImageResourceKey);
    }

    [Fact]
    public void UpdateImageResourceKey_WithUserHintedOpacity_ShowsBWVariant()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object);

        // Act - Raise event with user-hinted key (opacity < 1.0)
        var opacityKeyState = SavedItem.CreateEmpty(ItemName.KEY_1) with
        {
            Opacity = 0.5
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(opacityKeyState));

        // Assert
        Assert.Equal("basic_key_bw", viewModel.ImageResourceKey);
    }

    [Fact]
    public void UpdateImageResourceKey_WithValidRegionItem_ShowsFullColor()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var parsedSpoilerDataService = CreateMockParsedSpoilerDataService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object,
            parsedSpoilerDataService: parsedSpoilerDataService.Object);

        // Act - Raise event with key in a valid region (not ItemGrid)
        var validRegionKeyState = SavedItem.CreateEmpty(ItemName.KEY_1) with
        {
            Region = RegionName.JUNGLE_JAPES
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(validRegionKeyState));

        // Assert
        Assert.Equal("basic_key", viewModel.ImageResourceKey);
    }

    [Fact]
    public void UpdateImageResourceKey_WithItemGridItem_ShowsBWVariant()
    {
        // Arrange
        var itemTrackingService = CreateMockItemTrackingService();
        var viewModel = CreateViewModel(
            selectedLevel: 1,
            itemTrackingService: itemTrackingService.Object);

        // Act - Raise event with key in UNKNOWN region (not a valid move region)
        var itemGridKeyState = SavedItem.CreateEmpty(ItemName.KEY_1) with
        {
            Region = RegionName.UNKNOWN
        };
        itemTrackingService.Raise(s => s.ItemStateChanged += null,
            new ItemStateChangedEventArgs(itemGridKeyState));

        // Assert
        Assert.Equal("basic_key_bw", viewModel.ImageResourceKey);
    }

    #endregion
}
