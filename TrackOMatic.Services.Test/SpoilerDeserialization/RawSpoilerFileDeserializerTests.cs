using System.Text;

using TrackOMatic.Services.SpoilerDeserialization;

namespace TrackOMatic.Services.Test.SpoilerDeserialization;

/// <summary>
/// Unit tests for <see cref="RawSpoilerFileDeserializer"/>.
/// </summary>
public class RawSpoilerFileDeserializerTests
{
    private readonly RawSpoilerFileDeserializer _deserializer;
    private readonly string _tempDirectory = Path.Combine(Path.GetTempPath(), "SpoilerDeserializerTests");

    public RawSpoilerFileDeserializerTests()
    {
        _deserializer = new RawSpoilerFileDeserializer();

        // Create temp directory for test files
        if (!Directory.Exists(_tempDirectory))
        {
            Directory.CreateDirectory(_tempDirectory);
        }
    }

    #region Helper Methods

    private string GetTempFilePath(string fileName) => Path.Combine(_tempDirectory, fileName);

    private async Task WriteTempFileAsync(string fileName, string content)
    {
        string filePath = GetTempFilePath(fileName);
        await File.WriteAllTextAsync(filePath, content);
    }

    private void CleanupTempFile(string fileName)
    {
        string filePath = GetTempFilePath(fileName);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static string CreateMinimalValidSpoilerJson() => """
        {
            "Settings": {
                "Shockwave Shuffle": "never"
            },
            "Spoiler Hints Data": {
                "0": {
                    "level_name": "Jungle Japes",
                    "level_order": 0,
                    "vial_colors": ["Clear Vial"],
                    "points": 10,
                    "woth_count": 3
                },
                "starting_info": {
                    "krool_order": [1, 2, 3, 4, 5, 6, 0, 7],
                    "helm_order": [0, 1, 2, 3, 4],
                    "starting_kongs": [0],
                    "starting_keys": [],
                    "starting_moves": [],
                    "starting_moves_not_hintable": [],
                    "starting_moves_woth_count": 0,
                    "level_order": [0, 1, 2, 3, 4, 5, 6, 7],
                    "blocker_info": []
                },
                "point_spread": {
                    "kongs": 3,
                    "keys": 5
                }
            },
            "Items": {},
            "Item Pool": [],
            "Randomizer Version": "7.0"
        }
        """;

    #endregion

    #region DeserializeFromFileAsync Tests

    [Fact]
    public async Task DeserializeFromFileAsync_WithValidFile_ReturnsSuccessResult()
    {
        // Arrange
        string fileName = "valid_spoiler.json";
        string jsonContent = CreateMinimalValidSpoilerJson();
        await WriteTempFileAsync(fileName, jsonContent);

        try
        {
            string filePath = GetTempFilePath(fileName);

            // Act
            var result = await _deserializer.DeserializeFromFileAsync(filePath);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.True(result.IsSuccess);
                Assert.NotNull(result.Data);
                Assert.Null(result.ErrorMessage);
                Assert.NotNull(result.Data.HintData);
            });
        }
        finally
        {
            CleanupTempFile(fileName);
        }
    }

    [Fact]
    public async Task DeserializeFromFileAsync_WithNullFilePath_ReturnsFailureResult()
    {
        // Act
        var result = await _deserializer.DeserializeFromFileAsync(null!);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("null or empty", result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromFileAsync_WithEmptyFilePath_ReturnsFailureResult()
    {
        // Act
        var result = await _deserializer.DeserializeFromFileAsync("");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("null or empty", result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromFileAsync_WithNonExistentFile_ReturnsFailureResult()
    {
        // Act
        var result = await _deserializer.DeserializeFromFileAsync("/nonexistent/path/file.json");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("File not found", result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromFileAsync_WithInvalidJsonFile_ReturnsFailureResult()
    {
        // Arrange
        string fileName = "invalid_json.json";
        await WriteTempFileAsync(fileName, "{ invalid json content ]");

        try
        {
            string filePath = GetTempFilePath(fileName);

            // Act
            var result = await _deserializer.DeserializeFromFileAsync(filePath);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsSuccess);
                Assert.Null(result.Data);
                Assert.NotNull(result.ErrorMessage);
            });
        }
        finally
        {
            CleanupTempFile(fileName);
        }
    }

    [Fact]
    public async Task DeserializeFromFileAsync_WithCancellation_ReturnsCancelledResult()
    {
        // Arrange
        string fileName = "valid_spoiler.json";
        string jsonContent = CreateMinimalValidSpoilerJson();
        await WriteTempFileAsync(fileName, jsonContent);
        var cts = new CancellationTokenSource();

        try
        {
            string filePath = GetTempFilePath(fileName);
            cts.Cancel();

            // Act
            var result = await _deserializer.DeserializeFromFileAsync(filePath, cts.Token);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.False(result.IsSuccess);
                Assert.Null(result.Data);
                Assert.NotNull(result.ErrorMessage);
                Assert.Contains("cancelled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
            });
        }
        finally
        {
            CleanupTempFile(fileName);
            cts.Dispose();
        }
    }

    #endregion

    #region DeserializeFromStringAsync Tests

    [Fact]
    public async Task DeserializeFromStringAsync_WithValidJson_ReturnsSuccessResult()
    {
        // Arrange
        string jsonContent = CreateMinimalValidSpoilerJson();

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Null(result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithNullJsonContent_ReturnsFailureResult()
    {
        // Act
        var result = await _deserializer.DeserializeFromStringAsync(null!);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("null or empty", result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithEmptyJsonContent_ReturnsFailureResult()
    {
        // Act
        var result = await _deserializer.DeserializeFromStringAsync("");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("null or empty", result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithWhitespaceOnlyContent_ReturnsFailureResult()
    {
        // Act
        var result = await _deserializer.DeserializeFromStringAsync("   \n\t  ");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("null or empty", result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithInvalidJson_ReturnsFailureResult()
    {
        // Arrange
        string invalidJson = "{ this is not valid json ]";

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(invalidJson);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithValidJsonPreservesStructure()
    {
        // Arrange
        string jsonContent = CreateMinimalValidSpoilerJson();

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.HintData);
            Assert.NotNull(result.Data.HintData.StartingInfo);
            Assert.NotNull(result.Data.Settings);
            Assert.NotNull(result.Data.HintData.PointSpread);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithCancellation_ReturnsCancelledResult()
    {
        // Arrange
        string jsonContent = CreateMinimalValidSpoilerJson();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent, cts.Token);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.Null(result.Data);
            Assert.NotNull(result.ErrorMessage);
            Assert.Contains("cancelled", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
        });
    }

    #endregion

    #region Edge Cases & Data Validation Tests

    [Fact]
    public async Task DeserializeFromStringAsync_WithJsonNumberEdgeCases_HandlesCorrectly()
    {
        // Arrange
        string jsonContent = """
            {
                "Spoiler Hints Data": {
                    "0": {
                        "level_name": "Jungle Japes",
                        "level_order": 0,
                        "vial_colors": [],
                        "points": -1,
                        "woth_count": -1
                    },
                    "starting_info": {
                        "krool_order": [],
                        "helm_order": [],
                        "starting_kongs": [],
                        "starting_keys": [],
                        "starting_moves": [],
                        "starting_moves_not_hintable": [],
                        "starting_moves_woth_count": 0,
                        "level_order": [],
                        "blocker_info": []
                    }
                }
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.HintData);
            Assert.NotNull(result.Data.HintData.RegionDataDictionary);
            Assert.True(result.Data.HintData.RegionDataDictionary.TryGetValue(0, out var japesData));
            Assert.Equal(-1, japesData.Points);
            Assert.Equal(-1, japesData.WothCount);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithPartiallyNullFields_HandlesGracefully()
    {
        // Arrange
        string jsonContent = """
            {
                "Settings": null,
                "Spoiler Hints Data": null,
                "Items": null,
                "Item Pool": null,
                "Randomizer Version": null
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Null(result.Data.Settings);
            Assert.Null(result.Data.HintData);
            Assert.Null(result.Data.ItemPool);
            Assert.Null(result.Data.RandomizerVersion);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithExtraUnknownFields_IgnoresThem()
    {
        // Arrange
        string jsonContent = """
            {
                "Settings": {},
                "Spoiler Hints Data": {},
                "Items": {},
                "Item Pool": [],
                "Randomizer Version": "7.0",
                "UnknownField1": "value1",
                "UnknownField2": { "nested": "value" },
                "UnknownField3": [1, 2, 3]
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("7.0", result.Data.RandomizerVersion);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithComplexNestedStructure_DeserializesCorrectly()
    {
        // Arrange
        string jsonContent = """
            {
                "Spoiler Hints Data": {
                    "0": {
                        "level_name": "Jungle Japes",
                        "level_order": 0,
                        "vial_colors": ["Clear Vial", "Yellow Vial", "Red Vial"],
                        "points": 15,
                        "woth_count": 4
                    },
                    "1": {
                        "level_name": "Angry Aztec",
                        "level_order": 1,
                        "vial_colors": ["Blue Vial"],
                        "points": 20,
                        "woth_count": 5
                    },
                    "starting_info": {
                        "krool_order": [1, 2, 3, 4, 5, 6, 0, 7],
                        "helm_order": [0, 1, 2, 3, 4],
                        "starting_kongs": [0, 1],
                        "starting_keys": ["Key 1"],
                        "starting_moves": ["Diving"],
                        "starting_moves_not_hintable": ["Barrel Throwing"],
                        "starting_moves_woth_count": 2,
                        "level_order": [0, 1, 2, 3, 4, 5, 6, 7],
                        "blocker_info": []
                    },
                    "point_spread": {
                        "kongs": 3,
                        "keys": 5,
                        "guns": 2,
                        "instruments": 1
                    }
                }
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.NotNull(result.Data.HintData);
            Assert.NotNull(result.Data.HintData.RegionDataDictionary);
            Assert.True(result.Data.HintData.RegionDataDictionary.TryGetValue(0, out var japesData));
            Assert.True(result.Data.HintData.RegionDataDictionary.TryGetValue(1, out var aztecData));
            Assert.Equal("Jungle Japes", japesData.LevelName);
            Assert.Equal(3, japesData.VialColors?.Count);
            Assert.Equal("Angry Aztec", aztecData.LevelName);
            Assert.Single(aztecData.VialColors ?? []);
            Assert.NotNull(result.Data.HintData.StartingInfo);
            Assert.Equal(8, result.Data.HintData.StartingInfo.FinalBossOrder?.Count);
            Assert.NotNull(result.Data.HintData.PointSpread);
            Assert.Equal(4, result.Data.HintData.PointSpread.Count);
        });
    }

    #endregion

    #region Performance & Concurrency Tests

    [Fact]
    public async Task DeserializeFromStringAsync_WithLargeJson_CompletesSuccessfully()
    {
        // Arrange - Create a moderately large JSON payload
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine("  \"Spoiler Hints Data\": {");

        for (int i = 0; i < 9; i++)
        {
            sb.AppendLine($@"    ""{i}"": {{");
            sb.AppendLine($@"      ""level_name"": ""Region {i}"",");
            sb.AppendLine($@"      ""level_order"": {i},");
            sb.AppendLine(@"      ""vial_colors"": [""Clear Vial"", ""Yellow Vial""],");
            sb.AppendLine($@"      ""points"": {i * 10},");
            sb.AppendLine($@"      ""woth_count"": {i * 2}");
            sb.AppendLine(i < 8 ? "    }," : "    }");
        }

        sb.AppendLine("  }");
        sb.AppendLine("}");

        string jsonContent = sb.ToString();

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithConcurrentCalls_HandlesIndependently()
    {
        // Arrange
        string json1 = """
            {
                "Randomizer Version": "7.0",
                "Spoiler Hints Data": { "starting_info": {} }
            }
            """;
        string json2 = """
            {
                "Randomizer Version": "6.5",
                "Spoiler Hints Data": { "starting_info": {} }
            }
            """;

        // Act
        var task1 = _deserializer.DeserializeFromStringAsync(json1);
        var task2 = _deserializer.DeserializeFromStringAsync(json2);
        var results = await Task.WhenAll(task1, task2);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(results[0].IsSuccess);
            Assert.True(results[1].IsSuccess);
            Assert.Equal("7.0", results[0].Data?.RandomizerVersion);
            Assert.Equal("6.5", results[1].Data?.RandomizerVersion);
        });
    }

    #endregion

    #region Dictionary-Specific Tests

    [Fact]
    public async Task DeserializeFromStringAsync_RegionDataDictionary_ContainsCorrectRegionIndices()
    {
        // Arrange - Create JSON with multiple regions
        string jsonContent = """
            {
                "Spoiler Hints Data": {
                    "0": {
                        "level_name": "Jungle Japes",
                        "level_order": 0,
                        "vial_colors": [],
                        "points": 10,
                        "woth_count": 2
                    },
                    "1": {
                        "level_name": "Angry Aztec",
                        "level_order": 1,
                        "vial_colors": [],
                        "points": 12,
                        "woth_count": 3
                    },
                    "2": {
                        "level_name": "Frantic Factory",
                        "level_order": 2,
                        "vial_colors": [],
                        "points": 14,
                        "woth_count": 4
                    },
                    "starting_info": {}
                }
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.HintData?.RegionDataDictionary);
            Assert.Equal(3, result.Data.HintData.RegionDataDictionary.Count);
            Assert.True(result.Data.HintData.RegionDataDictionary.ContainsKey(0));
            Assert.True(result.Data.HintData.RegionDataDictionary.ContainsKey(1));
            Assert.True(result.Data.HintData.RegionDataDictionary.ContainsKey(2));
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_RegionDataDictionary_AllowsIterationOverRegions()
    {
        // Arrange - Create JSON with all 9 regions
        var sb = new StringBuilder();
        sb.AppendLine("""
            {
                "Spoiler Hints Data": {
            """);

        for (int i = 0; i < 9; i++)
        {
            sb.AppendLine($@"    ""{i}"": {{");
            sb.AppendLine($@"      ""level_name"": ""Region {i}"",");
            sb.AppendLine($@"      ""level_order"": {i},");
            sb.AppendLine($@"      ""vial_colors"": [],");
            sb.AppendLine($@"      ""points"": {i * 10},");
            sb.AppendLine($@"      ""woth_count"": {i}");
            sb.AppendLine(i < 8 ? "    }," : "    }");
        }

        sb.AppendLine("""
                },
                "starting_info": {}
            }
            """);

        string jsonContent = sb.ToString();

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.HintData?.RegionDataDictionary);
            Assert.Equal(9, result.Data.HintData.RegionDataDictionary.Count);

            // Verify we can iterate and access each region
            int count = 0;
            foreach (var kvp in result.Data.HintData.RegionDataDictionary)
            {
                Assert.NotNull(kvp.Value);
                Assert.Equal(count, kvp.Key); // Key should match iteration order
                Assert.Equal(count * 10, kvp.Value.Points);
                count++;
            }
            Assert.Equal(9, count);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_RegionDataDictionary_SupportsDirectKeyAccess()
    {
        // Arrange
        string jsonContent = """
            {
                "Spoiler Hints Data": {
                    "0": {
                        "level_name": "Jungle Japes",
                        "level_order": 0,
                        "vial_colors": [],
                        "points": 5,
                        "woth_count": 1
                    },
                    "3": {
                        "level_name": "Gloomy Galleon",
                        "level_order": 3,
                        "vial_colors": [],
                        "points": 15,
                        "woth_count": 5
                    },
                    "starting_info": {}
                }
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.HintData?.RegionDataDictionary);

            // Direct key access
            Assert.Contains("Japes", result.Data.HintData.RegionDataDictionary[0].LevelName);
            Assert.Contains("Galleon", result.Data.HintData.RegionDataDictionary[3].LevelName);
            Assert.Equal(5, result.Data.HintData.RegionDataDictionary[0].Points);
            Assert.Equal(15, result.Data.HintData.RegionDataDictionary[3].Points);
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_RegionDataDictionary_HandlesEmptyDictionary()
    {
        // Arrange - Create JSON with empty Spoiler Hints Data
        string jsonContent = """
            {
                "Spoiler Hints Data": {
                    "starting_info": {},
                    "point_spread": {}
                }
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.HintData);
            // RegionDataDictionary should be empty or null
            Assert.True(
                result.Data.HintData.RegionDataDictionary == null ||
                result.Data.HintData.RegionDataDictionary.Count == 0
            );
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_RegionDataDictionary_MaintainsIntKeyType()
    {
        // Arrange
        string jsonContent = """
            {
                "Spoiler Hints Data": {
                    "0": { "level_name": "Region 0", "level_order": 0, "vial_colors": [], "points": 0, "woth_count": 0 },
                    "5": { "level_name": "Region 5", "level_order": 5, "vial_colors": [], "points": 50, "woth_count": 5 },
                    "starting_info": {}
                }
            }
            """;

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data?.HintData?.RegionDataDictionary);

            // Keys should be integers (strongly typed)
            var keys = result.Data.HintData.RegionDataDictionary.Keys.ToList();
            Assert.Contains(0, keys);
            Assert.Contains(5, keys);

            // Verify the type is int
            foreach (var key in keys)
            {
                Assert.IsType<int>(key);
            }
        });
    }

    [Fact]
    public async Task DeserializeFromStringAsync_WithStringEncodedRegionData_DeserializesCorrectly()
    {
        // Arrange - Simulates fixture format where region data is stored as escaped JSON strings
        string jsonContent = "{\"Spoiler Hints Data\":{\"0\":\"{\\\"level_name\\\":\\\"Jungle Japes\\\",\\\"vial_colors\\\":[],\\\"points\\\":113,\\\"level_order\\\":0,\\\"woth_count\\\":-1}\",\"1\":\"{\\\"level_name\\\":\\\"Angry Aztec\\\",\\\"vial_colors\\\":[],\\\"points\\\":214,\\\"level_order\\\":1,\\\"woth_count\\\":-1}\",\"starting_info\":\"{\\\"krool_order\\\":[205,207,206],\\\"helm_order\\\":[4,3],\\\"starting_kongs\\\":[4,2],\\\"starting_keys\\\":[\\\"Key 3\\\"],\\\"starting_moves\\\":[\\\"Mini Monkey\\\"],\\\"starting_moves_not_hintable\\\":[\\\"Progressive Slam\\\"],\\\"starting_moves_woth_count\\\":-1}\",\"point_spread\":\"{\\\"kongs\\\":100,\\\"keys\\\":100,\\\"guns\\\":10,\\\"instruments\\\":1}\"}}";

        // Act
        var result = await _deserializer.DeserializeFromStringAsync(jsonContent);

        // Assert - First check if deserialization succeeded
        if (!result.IsSuccess)
        {
            throw new Exception($"Deserialization failed: {result.ErrorMessage}");
        }

        Assert.Multiple(() =>
        {
            Assert.NotNull(result.Data?.HintData);
            Assert.NotNull(result.Data.HintData.RegionDataDictionary);

            // Verify regions were deserialized
            Assert.Equal(2, result.Data.HintData.RegionDataDictionary.Count);

            // Verify region 0
            Assert.True(result.Data.HintData.RegionDataDictionary.ContainsKey(0));
            var region0 = result.Data.HintData.RegionDataDictionary[0];
            Assert.Equal("Jungle Japes", region0.LevelName);
            Assert.Equal(113, region0.Points);
            Assert.Equal(-1, region0.WothCount);

            // Verify region 1
            Assert.True(result.Data.HintData.RegionDataDictionary.ContainsKey(1));
            var region1 = result.Data.HintData.RegionDataDictionary[1];
            Assert.Equal("Angry Aztec", region1.LevelName);
            Assert.Equal(214, region1.Points);

            // Verify starting info was deserialized
            Assert.NotNull(result.Data.HintData.StartingInfo);
            Assert.NotNull(result.Data.HintData.StartingInfo.StartingKongs);
            Assert.Equal(2, result.Data.HintData.StartingInfo.StartingKongs.Count);

            // Verify point spread was deserialized
            Assert.NotNull(result.Data.HintData.PointSpread);
            Assert.Equal(100, result.Data.HintData.PointSpread["kongs"]);
        });
    }

    [Theory]
    [InlineData("183145-rap-sheet-spoilerlog.json", false, true)]
    [InlineData("799354-spoilerlog-gvm.json", true, false)]
    public async Task DeserializeFromFileAsync_WithActualFixtureFile_DeserializesSuccessfully(string fileName, bool hasVials, bool hasPoints)
    {
        // Arrange - Locate the fixture file using the test assembly location
        var assemblyLocation = typeof(RawSpoilerFileDeserializerTests).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine assembly directory");
        var fixturePath = Path.Combine(assemblyDir, "TestData", fileName);

        // Skip the test if fixture not found
        if (!File.Exists(fixturePath))
        {
            return; // Skip if fixture was not copied to output
        }

        // Act
        var result = await _deserializer.DeserializeFromFileAsync(fixturePath);

        // Assert
        if (!result.IsSuccess)
        {
            throw new Exception($"Fixture deserialization failed: {result.ErrorMessage}. Path: {fixturePath}");
        }

        Assert.Multiple(() =>
        {
            Assert.NotNull(result.Data?.HintData);
            Assert.NotNull(result.Data.HintData.RegionDataDictionary);

            // Verify at least some regions were deserialized
            Assert.True(result.Data.HintData.RegionDataDictionary.Count > 0, "Should have at least one region");

            // Verify starting info exists
            Assert.NotNull(result.Data.HintData.StartingInfo);

            // Verify point spread exists where appropriate.
            if (hasVials)
            {
                Assert.Null(result.Data.HintData.PointSpread);
            }
            if (hasPoints)
            {
                Assert.NotNull(result.Data.HintData.PointSpread);
            }
        });
    }

    #endregion
}
