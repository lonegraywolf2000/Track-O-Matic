using System.Reflection;
using System.Text.Json;

using TrackOMatic.Logic;
using TrackOMatic.Logic.Enums;

namespace TrackOMatic.Services.Test;

/// <summary>
/// Unit tests for UserSettingsService.
/// Tests JSON persistence, property change notifications, and enum conversions.
/// </summary>
public sealed class UserSettingsServiceTests : IDisposable
{
    private readonly string _testSettingsDir;
    private readonly string _testSettingsFile;
    private readonly JsonSerializerOptions _options;
    public UserSettingsServiceTests()
    {
        _testSettingsDir = Path.Combine(Path.GetTempPath(), $"Track-O-Matic-Tests-{Guid.NewGuid()}");
        _testSettingsFile = Path.Combine(_testSettingsDir, "user-settings.json");
        _options = new JsonSerializerOptions { WriteIndented = true };
        Directory.CreateDirectory(_testSettingsDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testSettingsDir))
        {
            Directory.Delete(_testSettingsDir, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    #region Initialization Tests

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var service = new UserSettingsService(_testSettingsFile);

        Assert.Multiple(() =>
        {
            // Display toggles
            Assert.False(service.TopMost);
            Assert.False(service.SongDisplay);
            Assert.True(service.CompactMode);
            Assert.False(service.HelmInLevelOrder);
            Assert.False(service.HelmDoors);
            Assert.False(service.AutoSortPathHints);
            Assert.False(service.ColoredBarrelPadMoves);
            Assert.True(service.Autotracking);

            // Collectibles
            Assert.False(service.ShowTotalBPs);
            Assert.False(service.ShowCompanyCoins);

            // Spoiler hints
            Assert.True(service.ShowKRoolOrder);
            Assert.True(service.ShowHelmOrder);

            // Broadcast
            Assert.False(service.BroadcastHelmKRool);
            Assert.False(service.BroadcastShopkeepers);
            Assert.False(service.BroadcastSongDisplay);
            Assert.Equal(BroadcastNumberLabel.WothCount, service.BroadcastNumberLabel);

            // HintDisplay
            Assert.Equal(HintDisplayMode.MultipathHints, service.HintDisplay);

            // Progressive hints
            Assert.False(service.ShowAmountForHints);

            // Hint search options
            Assert.False(service.EnemiesInAutofill);
        });
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void PropertyChange_RaisesPropertyChanged()
    {
        var service = new UserSettingsService(_testSettingsFile);
        var changedProperties = new List<string>();

        service.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != null)
            {
                changedProperties.Add(e.PropertyName);
            }
        };

        service.TopMost = true;
        service.SongDisplay = true;
        service.CompactMode = false;

        Assert.Multiple(() =>
        {
            Assert.Contains(nameof(service.TopMost), changedProperties);
            Assert.Contains(nameof(service.SongDisplay), changedProperties);
            Assert.Contains(nameof(service.CompactMode), changedProperties);
        });
    }

    [Fact]
    public void PropertyChange_UpdatesValue()
    {
        var service = new UserSettingsService(_testSettingsFile)
        {
            TopMost = true,
            SongDisplay = true,
            CompactMode = false
        };
        Assert.Multiple(() =>
        {
            Assert.True(service.TopMost);
            Assert.True(service.SongDisplay);
            Assert.False(service.CompactMode);
        });
    }

    [Fact]
    public void BooleanProperties_CanBeToggled()
    {
        var service = new UserSettingsService(_testSettingsFile);

        var boolProperties = new[]
        {
            nameof(service.TopMost), nameof(service.SongDisplay), nameof(service.CompactMode),
            nameof(service.HelmInLevelOrder), nameof(service.HelmDoors), nameof(service.AutoSortPathHints),
            nameof(service.ColoredBarrelPadMoves), nameof(service.Autotracking), nameof(service.ShowTotalBPs),
            nameof(service.ShowCompanyCoins), nameof(service.ShowKRoolOrder), nameof(service.ShowHelmOrder),
            nameof(service.BroadcastHelmKRool), nameof(service.BroadcastShopkeepers),
            nameof(service.BroadcastSongDisplay), nameof(service.ShowAmountForHints), nameof(service.EnemiesInAutofill)
        };

        foreach (var propName in boolProperties)
        {
            var prop = typeof(UserSettingsService).GetProperty(propName);
            Assert.NotNull(prop);

            var originalValue = (bool)prop!.GetValue(service)!;
            prop.SetValue(service, !originalValue);
            var newValue = (bool)prop.GetValue(service)!;

            Assert.NotEqual(originalValue, newValue);
        }
    }

    #endregion

    #region HintDisplayMode Tests

    [Fact]
    public void HintDisplay_DefaultIsMultipathHints()
    {
        var service = new UserSettingsService(_testSettingsFile);
        Assert.Equal(HintDisplayMode.MultipathHints, service.HintDisplay);
    }

    [Fact]
    public void HintDisplay_CanBeSetToAllEnumValues()
    {
        var service = new UserSettingsService(_testSettingsFile)
        {
            HintDisplay = HintDisplayMode.Off
        };
        Assert.Equal(HintDisplayMode.Off, service.HintDisplay);

        service.HintDisplay = HintDisplayMode.MultipathHints;
        Assert.Equal(HintDisplayMode.MultipathHints, service.HintDisplay);

        service.HintDisplay = HintDisplayMode.DirectItemHints;
        Assert.Equal(HintDisplayMode.DirectItemHints, service.HintDisplay);
    }

    [Fact]
    public void LegacyNameAttribute_ExistsOnEnumValues()
    {
        var offField = typeof(HintDisplayMode).GetField(nameof(HintDisplayMode.Off));
        var multipathField = typeof(HintDisplayMode).GetField(nameof(HintDisplayMode.MultipathHints));
        var directField = typeof(HintDisplayMode).GetField(nameof(HintDisplayMode.DirectItemHints));

        var offAttr = offField?.GetCustomAttribute<LegacyNameAttribute>();
        var multipathAttr = multipathField?.GetCustomAttribute<LegacyNameAttribute>();
        var directAttr = directField?.GetCustomAttribute<LegacyNameAttribute>();

        Assert.Multiple(() =>
        {
            Assert.NotNull(offAttr);
            Assert.NotNull(multipathAttr);
            Assert.NotNull(directAttr);

            Assert.Equal("Off", offAttr!.LegacyName);
            Assert.Equal("Multipath Hints", multipathAttr!.LegacyName);
            Assert.Equal("Direct Item Hints", directAttr!.LegacyName);
        });
    }

    #endregion

    #region BroadcastNumberLabel Tests

    [Fact]
    public void BroadcastNumberLabel_DefaultIsWothCount()
    {
        var service = new UserSettingsService(_testSettingsFile);
        Assert.Equal(BroadcastNumberLabel.WothCount, service.BroadcastNumberLabel);
    }

    [Fact]
    public void BroadcastNumberLabel_CanBeSetToAllEnumValues()
    {
        var service = new UserSettingsService(_testSettingsFile)
        {
            BroadcastNumberLabel = BroadcastNumberLabel.Points
        };
        Assert.Equal(BroadcastNumberLabel.Points, service.BroadcastNumberLabel);

        service.BroadcastNumberLabel = BroadcastNumberLabel.WothCount;
        Assert.Equal(BroadcastNumberLabel.WothCount, service.BroadcastNumberLabel);
    }

    [Fact]
    public void BroadcastNumberLabel_LegacyNameAttribute_ExistsOnEnumValues()
    {
        var pointsField = typeof(BroadcastNumberLabel).GetField(nameof(BroadcastNumberLabel.Points));
        var wothField = typeof(BroadcastNumberLabel).GetField(nameof(BroadcastNumberLabel.WothCount));

        var pointsAttr = pointsField?.GetCustomAttribute<LegacyNameAttribute>();
        var wothAttr = wothField?.GetCustomAttribute<LegacyNameAttribute>();

        Assert.Multiple(() =>
        {
            Assert.NotNull(pointsAttr);
            Assert.NotNull(wothAttr);

            Assert.Equal("Points", pointsAttr!.LegacyName);
            Assert.Equal("WOTH Count", wothAttr!.LegacyName);
        });
    }

    #endregion

    #region File Persistence Tests

    [Fact]
    public void SaveToFile_CreatesJsonFile()
    {
        var service = new UserSettingsService(_testSettingsFile)
        {
            TopMost = true
        }
        ;
        service.SaveToFile();

        Assert.True(File.Exists(_testSettingsFile));
    }

    [Fact]
    public void SaveToFile_ContainsValidJson()
    {
        var service = new UserSettingsService(_testSettingsFile)
        {
            TopMost = true,
            SongDisplay = false,
            CompactMode = true,
            HintDisplay = HintDisplayMode.DirectItemHints
        }
        ;
        service.SaveToFile();

        var json = File.ReadAllText(_testSettingsFile);
        Assert.Multiple(() =>
        {
            Assert.Contains("\"TopMost\"", json);
            Assert.Contains("true", json);
            Assert.Contains("\"SongDisplay\"", json);
            Assert.Contains("\"CompactMode\"", json);
            Assert.Contains("\"HintDisplay\"", json);
            Assert.Contains("DirectItemHints", json);
        });
    }

    [Fact]
    public void LoadFromFile_RestoresAllProperties()
    {
        // Write JSON with specific values
        var testData = new UserSettingsData
        {
            TopMost = true,
            SongDisplay = true,
            CompactMode = false,
            HelmInLevelOrder = true,
            HintDisplay = "DirectItemHints"
        }
        ;

        var json = JsonSerializer.Serialize(testData, _options);
        File.WriteAllText(_testSettingsFile, json);

        // Load with new service
        var service = new UserSettingsService(_testSettingsFile);

        // Verify properties were restored
        Assert.Multiple(() =>
        {
            Assert.True(service.TopMost);
            Assert.True(service.SongDisplay);
            Assert.False(service.CompactMode);
            Assert.True(service.HelmInLevelOrder);
            Assert.Equal(HintDisplayMode.DirectItemHints, service.HintDisplay);
        });
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullCycle_SetPropertiesAndReload()
    {
        var service = new UserSettingsService(_testSettingsFile)
        {
            TopMost = true,
            CompactMode = false,
            HelmInLevelOrder = true,
            HintDisplay = HintDisplayMode.DirectItemHints
        }
        ;

        // Create another service that loads from the same file
        var service2 = new UserSettingsService(_testSettingsFile);
        Assert.Multiple(() =>
        {
            Assert.Equal(service.TopMost, service2.TopMost);
            Assert.Equal(service.CompactMode, service2.CompactMode);
            Assert.Equal(service.HelmInLevelOrder, service2.HelmInLevelOrder);
            Assert.Equal(service.HintDisplay, service2.HintDisplay);
        });
    }

    #endregion
}
