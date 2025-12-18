using Moq;
using Xunit;
using Bibliotheque.Api.Services;
using Microsoft.AspNetCore.Hosting;

namespace Bibliotheque.Tests.Services;

/// <summary>
/// Tests pour ImageService
/// </summary>
public class ImageServiceTests
{
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly ImageService _imageService;

    public ImageServiceTests()
    {
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.ContentRootPath).Returns(Path.GetTempPath());
        _imageService = new ImageService(_envMock.Object);
    }

    [Fact]
    public void IImageService_DefinesExpectedMethods()
    {
        var interfaceType = typeof(IImageService);
        
        Assert.NotNull(interfaceType.GetMethod("SaveImageAsync"));
        Assert.NotNull(interfaceType.GetMethod("GetImageAsync"));
        Assert.NotNull(interfaceType.GetMethod("DeleteImageAsync"));
    }

    [Fact]
    public void ImageService_Constructor_CreatesInstance()
    {
        // Assert
        Assert.NotNull(_imageService);
    }

    [Fact]
    public async Task DeleteImageAsync_NonExistentFile_DoesNotThrow()
    {
        // Arrange
        var nonExistentFile = "non_existent_file_12345.jpg";

        // Act & Assert - should not throw
        await _imageService.DeleteImageAsync(nonExistentFile);
    }

    [Fact]
    public async Task GetImageAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange
        var nonExistentFile = "non_existent_file_12345.jpg";

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => _imageService.GetImageAsync(nonExistentFile)
        );
    }
}
