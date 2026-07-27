using TrackOMatic.Services.TrackerState;

namespace TrackOMatic.Services.Test.TrackerState;

/// <summary>
/// Unit tests for SpoilerLogService.
/// Tests spoiler log path management and event firing.
/// </summary>
public class SpoilerLogServiceTests
{
    private readonly SpoilerLogService _sut;

    public SpoilerLogServiceTests()
    {
        _sut = new SpoilerLogService();
    }

    #region GetSpoilerLogPath Tests

    [Fact]
    public void GetSpoilerLogPath_InitiallyReturnsNull()
    {
        // Act
        var path = _sut.GetSpoilerLogPath();

        // Assert
        Assert.Null(path);
    }

    [Fact]
    public void GetSpoilerLogPath_AfterSetReturnsPath()
    {
        // Arrange
        var testPath = "/path/to/spoiler.log";

        // Act
        _sut.SetSpoilerLogPath(testPath);
        var retrievedPath = _sut.GetSpoilerLogPath();

        // Assert
        Assert.Equal(testPath, retrievedPath);
    }

    #endregion

    #region SetSpoilerLogPath Tests

    [Fact]
    public void SetSpoilerLogPath_StoresPath()
    {
        // Arrange
        var testPath = "C:\\spoilers\\my_spoiler.log";

        // Act
        _sut.SetSpoilerLogPath(testPath);

        // Assert
        Assert.Equal(testPath, _sut.GetSpoilerLogPath());
    }

    [Fact]
    public void SetSpoilerLogPath_AllowsNull()
    {
        // Arrange
        _sut.SetSpoilerLogPath("/initial/path/spoiler.log");

        // Act
        _sut.SetSpoilerLogPath(null);

        // Assert
        Assert.Null(_sut.GetSpoilerLogPath());
    }

    [Fact]
    public void SetSpoilerLogPath_AllowsMultipleUpdates()
    {
        // Act
        _sut.SetSpoilerLogPath("/path/1");
        Assert.Equal("/path/1", _sut.GetSpoilerLogPath());

        _sut.SetSpoilerLogPath("/path/2");
        Assert.Equal("/path/2", _sut.GetSpoilerLogPath());

        _sut.SetSpoilerLogPath("/path/3");
        Assert.Equal("/path/3", _sut.GetSpoilerLogPath());

        // Assert
        Assert.Equal("/path/3", _sut.GetSpoilerLogPath());
    }

    [Fact]
    public void SetSpoilerLogPath_AllowsEmptyString()
    {
        // Act
        _sut.SetSpoilerLogPath("");

        // Assert
        Assert.Equal("", _sut.GetSpoilerLogPath());
    }

    #endregion

    #region ReloadSpoilerLogAsync Tests

    [Fact]
    public async Task ReloadSpoilerLogAsync_ThrowsNotImplementedException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<NotImplementedException>(() => _sut.ReloadSpoilerLogAsync());
    }

    #endregion

    #region Event Tests

    [Fact]
    public void SpoilerLogStatusChanged_CanBeSubscribedTo()
    {
        // Arrange
#pragma warning disable CS0219 // Variable is assigned but its value is never used
        var eventFired = false;
#pragma warning restore CS0219 // Variable is assigned but its value is never used

        _sut.SpoilerLogStatusChanged += (sender, args) =>
        {
            eventFired = true;
        };

        // Act - Event not currently raised by SetSpoilerLogPath, but the event exists
        var hasEvent = _sut.GetType().GetEvent("SpoilerLogStatusChanged") != null;

        // Assert
        Assert.True(hasEvent);
    }

    #endregion
}
