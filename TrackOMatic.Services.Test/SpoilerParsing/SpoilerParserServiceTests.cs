namespace TrackOMatic.Services.Test.SpoilerParsing;

/// <summary>
/// Unit tests for <see cref="SpoilerParserService"/>.
/// </summary>
public class SpoilerParserServiceTests
{
    private readonly SpoilerParserService _service = new();

    #region Error Handling Tests

    [Fact]
    public async Task DeserializeAndParseAsync_WithNonExistentFile_ReturnsFailure()
    {
        // Arrange
        var nonExistentPath = Path.Combine(Path.GetTempPath(), $"nonexistent_spoiler_{Guid.NewGuid()}.json");

        // Act
        var result = await _service.DeserializeAndParseAsync(nonExistentPath);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.False(result.IsSuccess);
            Assert.NotNull(result.ErrorMessage);
        });
    }

    [Fact]
    public async Task DeserializeAndParseAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";
        var tempFile = Path.Combine(Path.GetTempPath(), $"spoiler_invalid_{Guid.NewGuid()}.json");
        try
        {
            await File.WriteAllTextAsync(tempFile, invalidJson);

            // Act
            var result = await _service.DeserializeAndParseAsync(tempFile);

            // Assert
            Assert.False(result.IsSuccess);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }
    }

    [Fact]
    public async Task DeserializeAndParseAsync_WithCancellation_ReturnsCancelledResult()
    {
        // Arrange
        var json = """{"Settings": {}, "Spoiler Hints Data": {}}""";
        var tempFile = Path.Combine(Path.GetTempPath(), $"spoiler_cancel_{Guid.NewGuid()}.json");
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();
        try
        {
            await File.WriteAllTextAsync(tempFile, json);

            // Act
            var result = await _service.DeserializeAndParseAsync(tempFile, cancellationTokenSource.Token);

            // Assert
            Assert.False(result.IsSuccess);
        }
        finally
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }

            cancellationTokenSource.Dispose();
        }
    }

    [Fact]
    public async Task DeserializeAndParseFromStringAsync_WithInvalidJson_ReturnsFailure()
    {
        // Arrange
        var invalidJson = "{ invalid json }";

        // Act
        var result = await _service.DeserializeAndParseFromStringAsync(invalidJson);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeserializeAndParseFromStringAsync_WithNullContent_ReturnsFailure()
    {
        // Act
        var result = await _service.DeserializeAndParseFromStringAsync(null!);

        // Assert
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task DeserializeAndParseFromStringAsync_WithEmptyContent_ReturnsFailure()
    {
        // Arrange
        var emptyJson = "";

        // Act
        var result = await _service.DeserializeAndParseFromStringAsync(emptyJson);

        // Assert
        Assert.False(result.IsSuccess);
    }

    #endregion

    #region Fixture File Tests

    [Fact]
    public async Task DeserializeAndParseAsync_WithFixtureFile_DeserializesSuccessfully()
    {
        // Arrange
        var assemblyLocation = typeof(SpoilerParserServiceTests).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine assembly directory");
        var fixturePath = Path.Combine(assemblyDir, "TestData", "183145-rap-sheet-spoilerlog.json");

        // Skip if fixture not available
        if (!File.Exists(fixturePath))
        {
            return;
        }

        // Act
        var result = await _service.DeserializeAndParseAsync(fixturePath);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess, $"Failed to parse fixture: {result.ErrorMessage}");
            Assert.NotNull(result.Data);
        });
    }

    [Fact]
    public async Task DeserializeAndParseAsync_WithSecondFixtureFile_DeserializesSuccessfully()
    {
        // Arrange
        var assemblyLocation = typeof(SpoilerParserServiceTests).Assembly.Location;
        var assemblyDir = Path.GetDirectoryName(assemblyLocation) ?? throw new InvalidOperationException("Could not determine assembly directory");
        var fixturePath = Path.Combine(assemblyDir, "TestData", "799354-spoilerlog-gvm.json");

        // Skip if fixture not available
        if (!File.Exists(fixturePath))
        {
            return;
        }

        // Act
        var result = await _service.DeserializeAndParseAsync(fixturePath);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.True(result.IsSuccess, $"Failed to parse fixture: {result.ErrorMessage}");
            Assert.NotNull(result.Data);
        });
    }

    #endregion
}
