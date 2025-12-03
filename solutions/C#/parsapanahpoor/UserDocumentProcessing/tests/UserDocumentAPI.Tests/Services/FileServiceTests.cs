using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using UserDocumentAPI.Services;
using Xunit;
using FluentAssertions;

namespace UserDocumentAPI.Tests.Services;

public class FileServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly FileService _sut;

    public FileServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);

        var options = Options.Create(new FileStorageOptions
        {
            UploadPath = Path.Combine(_testDirectory, "uploads"),
            PdfPath = Path.Combine(_testDirectory, "pdfs")
        });

        var logger = new Mock<ILogger<FileService>>();
        _sut = new FileService(logger.Object, options);
    }

    [Fact]
    public async Task SaveFileAsync_ValidFile_ReturnsFileInfo()
    {
        // Arrange
        var content = "Test file content"u8.ToArray();
        var fileName = "test.docx";
        var fileMock = CreateMockFormFile(content, fileName);

        // Act
        var (storedFileName, filePath) = await _sut.SaveFileAsync(fileMock.Object);

        // Assert
        storedFileName.Should().NotBeNullOrEmpty();
        storedFileName.Should().EndWith(".docx");
        filePath.Should().NotBeNullOrEmpty();
        File.Exists(filePath).Should().BeTrue();
    }

    [Fact]
    public async Task SaveFileAsync_ValidFile_SavesContentCorrectly()
    {
        // Arrange
        var content = "Test file content for verification"u8.ToArray();
        var fileName = "content-test.txt";
        var fileMock = CreateMockFormFile(content, fileName);

        // Act
        var (_, filePath) = await _sut.SaveFileAsync(fileMock.Object);

        // Assert
        var savedContent = await File.ReadAllBytesAsync(filePath);
        savedContent.Should().BeEquivalentTo(content);
    }

    [Fact]
    public async Task SaveFileAsync_GeneratesUniqueFileName()
    {
        // Arrange
        var content = "Test"u8.ToArray();
        var fileName = "duplicate.docx";
        var fileMock1 = CreateMockFormFile(content, fileName);
        var fileMock2 = CreateMockFormFile(content, fileName);

        // Act
        var (storedFileName1, _) = await _sut.SaveFileAsync(fileMock1.Object);
        var (storedFileName2, _) = await _sut.SaveFileAsync(fileMock2.Object);

        // Assert
        storedFileName1.Should().NotBe(storedFileName2);
    }

    [Fact]
    public async Task ConvertToPdfAsync_ValidFile_CreatesSimulatedPdf()
    {
        // Arrange
        var uploadsPath = Path.Combine(_testDirectory, "uploads");
        Directory.CreateDirectory(uploadsPath);
        var originalPath = Path.Combine(uploadsPath, "test.docx");
        await File.WriteAllTextAsync(originalPath, "test content");
        var pdfPath = Path.Combine(_testDirectory, "pdfs", "test.pdf");

        // Act
        var result = await _sut.ConvertToPdfAsync(originalPath, pdfPath);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ConvertToPdfAsync_NonExistentFile_ReturnsFalse()
    {
        // Arrange
        var originalPath = Path.Combine(_testDirectory, "nonexistent.docx");
        var pdfPath = Path.Combine(_testDirectory, "pdfs", "nonexistent.pdf");

        // Act
        var result = await _sut.ConvertToPdfAsync(originalPath, pdfPath);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_DeletesSuccessfully()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "temp.txt");
        await File.WriteAllTextAsync(filePath, "test");

        // Act
        await _sut.DeleteFileAsync(filePath);

        // Assert
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistingFile_DoesNotThrow()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "nonexistent.txt");

        // Act
        var action = async () => await _sut.DeleteFileAsync(filePath);

        // Assert
        await action.Should().NotThrowAsync();
    }

    [Fact]
    public async Task GetOldFilesAsync_ReturnsFilesOlderThanThreshold()
    {
        // Arrange
        var uploadsPath = Path.Combine(_testDirectory, "uploads");
        var testFile = Path.Combine(uploadsPath, "old-file.txt");
        await File.WriteAllTextAsync(testFile, "old content");
        
        // Set file creation time to 10 days ago
        File.SetCreationTime(testFile, DateTime.Now.AddDays(-10));

        var threshold = DateTime.UtcNow.AddDays(-7);

        // Act
        var oldFiles = await _sut.GetOldFilesAsync(threshold);

        // Assert
        oldFiles.Should().Contain(f => f.Contains("old-file.txt"));
    }

    private Mock<IFormFile> CreateMockFormFile(byte[] content, string fileName)
    {
        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(content);

        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.ContentType).Returns("application/octet-stream");
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns((Stream stream, CancellationToken _) =>
            {
                ms.Position = 0;
                return ms.CopyToAsync(stream);
            });

        return fileMock;
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
            Directory.Delete(_testDirectory, true);
    }
}
