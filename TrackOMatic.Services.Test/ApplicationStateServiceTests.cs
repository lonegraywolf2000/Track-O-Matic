namespace TrackOMatic.Services.Test;

using System.Text.Json;

using TrackOMatic.Logic.Enums;

/// <summary>
/// Unit tests for ApplicationStateService.
/// Tests JSON persistence and property change notifications.
/// </summary>
public sealed class ApplicationStateServiceTests : IDisposable
{
    private readonly string _testStateDir;
    private readonly string _testStateFile;
    private readonly JsonSerializerOptions _options;

    public ApplicationStateServiceTests()
    {
        _testStateDir = Path.Combine(Path.GetTempPath(), $"TrackOMatic-State-Tests-{Guid.NewGuid()}");
        _testStateFile = Path.Combine(_testStateDir, "app-state.json");
        _options = new JsonSerializerOptions { WriteIndented = true };
        Directory.CreateDirectory(_testStateDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStateDir))
        {
            Directory.Delete(_testStateDir, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    #region Initialization Tests

    [Fact]
    public void Constructor_InitializesWithDefaults()
    {
        var service = new ApplicationStateService(_testStateFile);

        Assert.Multiple(() =>
        {
            Assert.Equal(100, service.WindowX);
            Assert.Equal(100, service.WindowY);
            Assert.Equal(1800, service.DesiredWidth);
            Assert.Equal(820, service.DesiredHeight);
            Assert.Equal(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), service.LastFolderPath);
        });
    }

    #endregion

    #region Property Change Tests

    [Fact]
    public void PropertyChange_RaisesPropertyChanged()
    {
        var service = new ApplicationStateService(_testStateFile);
        var changedProperties = new List<string>();

        service.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName != null)
            {
                changedProperties.Add(e.PropertyName);
            }
        };

        service.WindowX = 200;
        service.WindowY = 300;
        service.DesiredWidth = 1920;

        Assert.Multiple(() =>
        {
            Assert.Contains(nameof(service.WindowX), changedProperties);
            Assert.Contains(nameof(service.WindowY), changedProperties);
            Assert.Contains(nameof(service.DesiredWidth), changedProperties);
        });
    }

    [Fact]
    public void DoubleProperties_CanBeChanged()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            WindowX = 500.5,
            WindowY = 250.75,
            DesiredWidth = 1280.0,
            DesiredHeight = 720.0
        };

        Assert.Multiple(() =>
        {
            Assert.Equal(500.5, service.WindowX);
            Assert.Equal(250.75, service.WindowY);
            Assert.Equal(1280.0, service.DesiredWidth);
            Assert.Equal(720.0, service.DesiredHeight);
        });
    }

    [Fact]
    public void StringProperty_CanBeChanged()
    {
        var service = new ApplicationStateService(_testStateFile);

        var testPath = @"C:\Users\Test\Documents";
        service.LastFolderPath = testPath;
        Assert.Equal(testPath, service.LastFolderPath);

        service.LastFolderPath = "";
        Assert.Equal("", service.LastFolderPath);
    }

    [Fact]
    public void WindowState_CanBeSetTogether()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            WindowX = 1024,
            WindowY = 512,
            DesiredWidth = 2048,
            DesiredHeight = 1536
        };

        Assert.Multiple(() =>
        {
            Assert.Equal(1024, service.WindowX);
            Assert.Equal(512, service.WindowY);
            Assert.Equal(2048, service.DesiredWidth);
            Assert.Equal(1536, service.DesiredHeight);
        });
    }

    #endregion

    #region File Persistence Tests

    [Fact]
    public void SaveToFile_CreatesJsonFile()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            WindowX = 200
        };
        service.SaveToFile();

        Assert.True(File.Exists(_testStateFile));
    }

    [Fact]
    public void SaveToFile_ContainsValidJson()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            WindowX = 300,
            WindowY = 400,
            DesiredWidth = 1920,
            DesiredHeight = 1080,
            LastFolderPath = @"C:\Test"
        };
        service.SaveToFile();

        var json = File.ReadAllText(_testStateFile);

        Assert.Multiple(() =>
        {
            Assert.Contains("\"WindowX\"", json);
            Assert.Contains("300", json);
            Assert.Contains("\"WindowY\"", json);
            Assert.Contains("400", json);
            Assert.Contains("\"DesiredWidth\"", json);
            Assert.Contains("1920", json);
            Assert.Contains("\"DesiredHeight\"", json);
            Assert.Contains("1080", json);
            Assert.Contains("\"LastFolderPath\"", json);
        });
    }

    [Fact]
    public void LoadFromFile_RestoresAllProperties()
    {
        // Write JSON with specific values
        var testData = new ApplicationStateData
        {
            WindowX = 750,
            WindowY = 850,
            DesiredWidth = 1600,
            DesiredHeight = 900,
            LastFolderPath = @"C:\Custom\Path"
        }
           ;

        var json = JsonSerializer.Serialize(testData, _options);
        File.WriteAllText(_testStateFile, json);

        // Load with new service
        var service = new ApplicationStateService(_testStateFile);

        // Verify properties were restored
        Assert.Multiple(() =>
        {
            Assert.Equal(750, service.WindowX);
            Assert.Equal(850, service.WindowY);
            Assert.Equal(1600, service.DesiredWidth);
            Assert.Equal(900, service.DesiredHeight);
            Assert.Equal(@"C:\Custom\Path", service.LastFolderPath);
        });
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FullCycle_SetStateAndReload()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            WindowX = 1024,
            WindowY = 512,
            DesiredWidth = 2048,
            DesiredHeight = 1536,
            LastFolderPath = @"D:\Projects"
        }
           ;

        // Create another service that loads from the same file
        var service2 = new ApplicationStateService(_testStateFile);
        Assert.Multiple(() =>
        {
            Assert.Equal(service.WindowX, service2.WindowX);
            Assert.Equal(service.WindowY, service2.WindowY);
            Assert.Equal(service.DesiredWidth, service2.DesiredWidth);
            Assert.Equal(service.DesiredHeight, service2.DesiredHeight);
            Assert.Equal(service.LastFolderPath, service2.LastFolderPath);
        });
    }

    [Fact]
    public void MultipleChanges_AllSavedCorrectly()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            WindowX = 100,
            WindowY = 200,
            DesiredWidth = 1600,
            DesiredHeight = 900
        };

        service.WindowX = 150;
        service.DesiredWidth = 1920;

        // Create a new service and load
        var service2 = new ApplicationStateService(_testStateFile);
        Assert.Multiple(() =>
        {
            Assert.Equal(150, service2.WindowX);
            Assert.Equal(200, service2.WindowY);
            Assert.Equal(1920, service2.DesiredWidth);
            Assert.Equal(900, service2.DesiredHeight);
        });
    }

    #endregion

    #region Progressive Hint Tests

    [Fact]
    public void ProgressiveHintItem_DefaultIsGoldenBanana()
    {
        var service = new ApplicationStateService(_testStateFile);
        Assert.Equal(ItemType.GOLDEN_BANANA, service.ProgressiveHintItem);
    }

    [Fact]
    public void ProgressiveHintItem_CanBeSetToDifferentValues()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            ProgressiveHintItem = ItemType.MISC
        };
        Assert.Equal(ItemType.MISC, service.ProgressiveHintItem);

        service.ProgressiveHintItem = ItemType.GUN;
        Assert.Equal(ItemType.GUN, service.ProgressiveHintItem);

        service.ProgressiveHintItem = ItemType.RAINBOW_COIN;
        Assert.Equal(ItemType.RAINBOW_COIN, service.ProgressiveHintItem);
    }

    [Fact]
    public void ProgressiveHintCap_DefaultIs50()
    {
        var service = new ApplicationStateService(_testStateFile);
        Assert.Equal(50, service.ProgressiveHintCap);
    }

    [Fact]
    public void ProgressiveHintCap_CanBeChanged()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            ProgressiveHintCap = 100
        };
        Assert.Equal(100, service.ProgressiveHintCap);

        service.ProgressiveHintCap = 25;
        Assert.Equal(25, service.ProgressiveHintCap);
    }

    [Fact]
    public void ProgressiveHint_LoadFromFile_RestoresAllProperties()
    {
        // Write JSON with specific values
        var testData = new ApplicationStateData
        {
            WindowX = 750,
            WindowY = 850,
            DesiredWidth = 1600,
            DesiredHeight = 900,
            LastFolderPath = @"C:\Custom\Path",
            ProgressiveHintItem = 5,  // ItemType.INSTRUMENT
            ProgressiveHintCap = 75
        };

        var json = JsonSerializer.Serialize(testData, _options);
        File.WriteAllText(_testStateFile, json);

        // Load with new service
        var service = new ApplicationStateService(_testStateFile);

        // Verify progressive hint properties were restored
        Assert.Multiple(() =>
        {
            Assert.Equal(ItemType.INSTRUMENT, service.ProgressiveHintItem);
            Assert.Equal(75, service.ProgressiveHintCap);
        });
    }

    [Fact]
    public void ProgressiveHint_FullCycle_SetAndReload()
    {
        var service = new ApplicationStateService(_testStateFile)
        {
            ProgressiveHintItem = ItemType.BARREL_MOVE,
            ProgressiveHintCap = 99
        };

        // Create another service that loads from the same file
        var service2 = new ApplicationStateService(_testStateFile);

        Assert.Multiple(() =>
        {
            Assert.Equal(service.ProgressiveHintItem, service2.ProgressiveHintItem);
            Assert.Equal(service.ProgressiveHintCap, service2.ProgressiveHintCap);
        });
    }

    #endregion
}
