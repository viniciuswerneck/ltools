using LTools.Core.Services;

namespace LTools.Tests;

public class LaravelDetectorTests
{
    private readonly LaravelDetector _detector = new();

    [Fact]
    public void IsLaravelProject_ReturnsFalse_ForInvalidPath()
    {
        var result = _detector.IsLaravelProject("");
        Assert.False(result);
    }

    [Fact]
    public void IsLaravelProject_ReturnsFalse_ForNonExistentPath()
    {
        var result = _detector.IsLaravelProject(@"C:\NonExistent\Path");
        Assert.False(result);
    }

    [Fact]
    public async Task DetectAsync_ReturnsNull_ForInvalidPath()
    {
        var result = await _detector.DetectAsync("");
        Assert.Null(result);
    }

    [Fact]
    public async Task ScanAsync_ReturnsEmpty_ForInvalidPath()
    {
        var result = await _detector.ScanAsync("");
        Assert.Empty(result);
    }
}
