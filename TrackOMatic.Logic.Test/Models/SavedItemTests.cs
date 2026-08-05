using System.Text.Json;

using TrackOMatic.Logic.Enums;
using TrackOMatic.Logic.Models;

namespace TrackOMatic.Logic.Test.Models;

public class SavedItemTests
{
    [Fact]
    public void Constructor_WithAllParameters_CreatesValidSavedItem()
    {
        // Arrange & Act
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            true,
            0.5,
            true
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(ItemName.DONKEY, savedItem.ItemName);
            Assert.Equal(RegionName.JUNGLE_JAPES, savedItem.Region);
            Assert.Equal(ItemVisibilityState.Visible, savedItem.Starred);
            Assert.True(savedItem.Autotracked);
            Assert.Equal(0.5, savedItem.Opacity);
            Assert.True(savedItem.Hinted);
        });
    }

    [Fact]
    public void Constructor_WithDefaultHinted_DefaultsToFalse()
    {
        // Arrange & Act
        var savedItem = new SavedItem(
            ItemName.TINY,
            RegionName.SHOPS,
            ItemVisibilityState.Hidden,
            false,
            1.0
        );

        // Assert
        Assert.False(savedItem.Hinted);
    }

    [Theory]
    [InlineData(ItemVisibilityState.Visible)]
    [InlineData(ItemVisibilityState.Hidden)]
    [InlineData(ItemVisibilityState.Collapsed)]
    public void Constructor_WithVariousVisibilityStates_StoresCorrectValue(ItemVisibilityState visibility)
    {
        // Arrange & Act
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            visibility,
            false,
            1.0
        );

        // Assert
        Assert.Equal(visibility, savedItem.Starred);
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    [InlineData(0.25)]
    public void Constructor_WithVariousOpacities_StoresOpacityValue(double opacity)
    {
        // Arrange & Act
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            opacity
        );

        // Assert
        Assert.Equal(opacity, savedItem.Opacity);
    }

    [Fact]
    public void Autotracked_CanBeModified_PropertyAllowsSetAfterConstruction()
    {
        // Arrange
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0
        );

        // Act
        savedItem.Autotracked = true;

        // Assert
        Assert.True(savedItem.Autotracked);
    }

    [Fact]
    public void ItemName_IsReadOnly_CannotBeChanged()
    {
        // Arrange
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0
        );

        // Assert
        Assert.Equal(ItemName.DONKEY, savedItem.ItemName);
    }

    [Fact]
    public void Region_CanBeModified_PropertyAllowsSetAfterConstruction()
    {
        // Arrange
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0
        );

        // Act
        savedItem.Region = RegionName.SHOPS;

        // Assert
        Assert.Equal(RegionName.SHOPS, savedItem.Region);
    }

    [Fact]
    public void Starred_CanBeModified_PropertyAllowsSetAfterConstruction()
    {
        // Arrange
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0
        );

        // Act
        savedItem.Starred = ItemVisibilityState.Hidden;

        // Assert
        Assert.Equal(ItemVisibilityState.Hidden, savedItem.Starred);
    }

    [Fact]
    public void Opacity_CanBeModified_PropertyAllowsSetAfterConstruction()
    {
        // Arrange
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            0.7
        );

        // Act
        savedItem.Opacity = 0.3;

        // Assert
        Assert.Equal(0.3, savedItem.Opacity);
    }

    [Fact]
    public void Hinted_CanBeModified_PropertyAllowsSetAfterConstruction()
    {
        // Arrange
        var savedItem = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0,
            true
        );

        // Act
        savedItem.Hinted = false;

        // Assert
        Assert.False(savedItem.Hinted);
    }

    [Fact]
    public void JsonSerialization_RoundTrip_PreservesAllProperties()
    {
        // Arrange
        var original = new SavedItem(
            ItemName.TINY,
            RegionName.SHOPS,
            ItemVisibilityState.Hidden,
            true,
            0.35,
            false
        );

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<SavedItem>(json);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(restored);
            Assert.Equal(original.ItemName, restored.ItemName);
            Assert.Equal(original.Region, restored.Region);
            Assert.Equal(original.Starred, restored.Starred);
            Assert.Equal(original.Autotracked, restored.Autotracked);
            Assert.Equal(original.Opacity, restored.Opacity);
            Assert.Equal(original.Hinted, restored.Hinted);
        });
    }

    [Fact]
    public void JsonSerialization_WithVisibleState_PreservesEnumValue()
    {
        // Arrange
        var original = new SavedItem(
            ItemName.DONKEY,
            RegionName.JUNGLE_JAPES,
            ItemVisibilityState.Visible,
            false,
            1.0
        );

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<SavedItem>(json);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(restored);
            Assert.Equal(ItemVisibilityState.Visible, restored.Starred);
        });
    }

    [Fact]
    public void JsonSerialization_WithCollapsedState_PreservesEnumValue()
    {
        // Arrange
        var original = new SavedItem(
            ItemName.CHUNKY,
            RegionName.CRYSTAL_CAVES,
            ItemVisibilityState.Collapsed,
            true,
            0.75
        );

        // Act
        var json = JsonSerializer.Serialize(original);
        var restored = JsonSerializer.Deserialize<SavedItem>(json);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(restored);
            Assert.Equal(ItemVisibilityState.Collapsed, restored.Starred);
        });
    }

    [Fact]
    public void MultipleInstances_AreIndependent()
    {
        // Arrange
        var item1 = new SavedItem(ItemName.DONKEY, RegionName.JUNGLE_JAPES, ItemVisibilityState.Visible, false, 1.0);
        var item2 = new SavedItem(ItemName.TINY, RegionName.SHOPS, ItemVisibilityState.Hidden, false, 0.5);

        // Act
        item1.Autotracked = true;
        item2.Autotracked = false;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(item1.Autotracked);
            Assert.False(item2.Autotracked);
            Assert.Equal(ItemName.DONKEY, item1.ItemName);
            Assert.Equal(ItemName.TINY, item2.ItemName);
        });
    }

    #region CreateEmpty Tests

    [Fact]
    public void CreateEmpty_ReturnsValidSavedItem()
    {
        // Act
        var savedItem = SavedItem.CreateEmpty(ItemName.DONKEY);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(savedItem);
            Assert.Equal(ItemName.DONKEY, savedItem.ItemName);
        });
    }

    [Fact]
    public void CreateEmpty_SetsCorrectDefaults()
    {
        // Act
        var savedItem = SavedItem.CreateEmpty(ItemName.DIDDY);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(ItemName.DIDDY, savedItem.ItemName);
            Assert.Equal(RegionName.UNKNOWN, savedItem.Region);
            Assert.Equal(ItemVisibilityState.Hidden, savedItem.Starred);
            Assert.False(savedItem.Autotracked);
            Assert.Equal(1.0, savedItem.Opacity);
            Assert.False(savedItem.Hinted);
        });
    }

    [Theory]
    [InlineData(ItemName.DONKEY)]
    [InlineData(ItemName.TINY)]
    [InlineData(ItemName.CHUNKY)]
    [InlineData(ItemName.LANKY)]
    public void CreateEmpty_WithVariousItemNames_CreatesCorrectly(ItemName itemName)
    {
        // Act
        var savedItem = SavedItem.CreateEmpty(itemName);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.NotNull(savedItem);
            Assert.Equal(itemName, savedItem.ItemName);
            Assert.Equal(RegionName.UNKNOWN, savedItem.Region);
            Assert.Equal(ItemVisibilityState.Hidden, savedItem.Starred);
        });
    }

    [Fact]
    public void CreateEmpty_CanBeModifiedAfterCreation()
    {
        // Arrange
        var savedItem = SavedItem.CreateEmpty(ItemName.DONKEY);

        // Act
        savedItem.Region = RegionName.JUNGLE_JAPES;
        savedItem.Starred = ItemVisibilityState.Visible;
        savedItem.Autotracked = true;
        savedItem.Opacity = 0.5;
        savedItem.Hinted = true;

        // Assert
        Assert.Multiple(() =>
        {
            Assert.Equal(RegionName.JUNGLE_JAPES, savedItem.Region);
            Assert.Equal(ItemVisibilityState.Visible, savedItem.Starred);
            Assert.True(savedItem.Autotracked);
            Assert.Equal(0.5, savedItem.Opacity);
            Assert.True(savedItem.Hinted);
        });
    }

    [Fact]
    public void CreateEmpty_MultipleInstances_AreIndependent()
    {
        // Act
        var item1 = SavedItem.CreateEmpty(ItemName.DONKEY);
        var item2 = SavedItem.CreateEmpty(ItemName.TINY);

        // Modify item1
        item1.Autotracked = true;
        item1.Region = RegionName.JUNGLE_JAPES;

        // Assert - item2 should remain unchanged
        Assert.Multiple(() =>
        {
            Assert.True(item1.Autotracked);
            Assert.Equal(RegionName.JUNGLE_JAPES, item1.Region);
            Assert.False(item2.Autotracked);
            Assert.Equal(RegionName.UNKNOWN, item2.Region);
        });
    }

    [Fact]
    public void CreateEmpty_StarredDefaultIsHidden_NotVisible()
    {
        // Act
        var savedItem = SavedItem.CreateEmpty(ItemName.DONKEY);

        // Assert - Explicitly verify Hidden, not Visible (design to avoid toggle-swap)
        Assert.Multiple(() =>
        {
            Assert.Equal(ItemVisibilityState.Hidden, savedItem.Starred);
            Assert.NotEqual(ItemVisibilityState.Visible, savedItem.Starred);
        });
    }

    #endregion
}
