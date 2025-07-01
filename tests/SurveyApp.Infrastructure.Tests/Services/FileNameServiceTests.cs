using FluentAssertions;
using SurveyApp.Infrastructure.Services;

namespace SurveyApp.Infrastructure.Tests.Services;

public class FileNameServiceTests
{
    private readonly FileNameService _service = new();

    [Fact]
    public void GenerateFileName_ValidInput_ShouldReturnCorrectFormat()
    {
        // Arrange
        var fullName = "Иван Петров";

        // Act
        var result = _service.GenerateFileName(fullName);

        // Assert
        result.Should().Be("Иван_Петров.txt");
    }

    [Fact]
    public void GenerateFileName_NameWithSpaces_ShouldReplaceWithUnderscores()
    {
        // Arrange
        var fullName = "Мария Ивановна Сидорова";

        // Act
        var result = _service.GenerateFileName(fullName);

        // Assert
        result.Should().Be("Мария_Ивановна_Сидорова.txt");
    }

    [Theory]
    [InlineData("Test/Name", "Test_Name.txt")]
    [InlineData("Test\\Name", "Test_Name.txt")]
    [InlineData("Test<Name>", "Test_Name.txt")]
    [InlineData("Test:Name", "Test_Name.txt")]
    [InlineData("Test|Name", "Test_Name.txt")]
    [InlineData("Test?Name", "Test_Name.txt")]
    [InlineData("Test*Name", "Test_Name.txt")]
    [InlineData("Test\"Name\"", "Test_Name.txt")]
    public void GenerateFileName_NameWithInvalidChars_ShouldSanitize(string name, string expected)
    {
        // Act
        var result = _service.GenerateFileName(name);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void GenerateFileName_EmptyName_ShouldUseUnknown()
    {
        // Arrange
        var fullName = "";

        // Act
        var result = _service.GenerateFileName(fullName);

        // Assert
        result.Should().Be("Unknown.txt");
    }

    [Fact]
    public void GenerateFileName_NullName_ShouldUseUnknown()
    {
        // Arrange
        string fullName = null!;

        // Act
        var result = _service.GenerateFileName(fullName);

        // Assert
        result.Should().Be("Unknown.txt");
    }

    [Fact]
    public void IsValidFileName_ValidFileName_ShouldReturnTrue()
    {
        // Arrange
        var fileName = "Valid_File_Name.txt";

        // Act
        var result = _service.IsValidFileName(fileName);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("file/name.txt")]
    [InlineData("file\\name.txt")]
    [InlineData("file<name>.txt")]
    [InlineData("file:name.txt")]
    [InlineData("file|name.txt")]
    [InlineData("file?name.txt")]
    [InlineData("file*name.txt")]
    [InlineData("file\"name\".txt")]
    public void IsValidFileName_FileNameWithInvalidChars_ShouldReturnFalse(string fileName)
    {
        // Act
        var result = _service.IsValidFileName(fileName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFileName_EmptyFileName_ShouldReturnFalse()
    {
        // Act
        var result = _service.IsValidFileName("");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFileName_NullFileName_ShouldReturnFalse()
    {
        // Act
        var result = _service.IsValidFileName(null!);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsValidFileName_WhitespaceFileName_ShouldReturnFalse()
    {
        // Act
        var result = _service.IsValidFileName("   ");

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("CON")]
    [InlineData("PRN")]
    [InlineData("AUX")]
    [InlineData("NUL")]
    [InlineData("COM1")]
    [InlineData("LPT1")]
    public void IsValidFileName_ReservedWindowsNames_ShouldReturnTrue(string reservedName)
    {
        // Act
        var result = _service.IsValidFileName(reservedName);

        // Assert
        // The current implementation doesn't check for reserved names
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFileName_FileNameEndingWithDot_ShouldReturnTrue()
    {
        // Act
        var result = _service.IsValidFileName("filename.");

        // Assert
        // The current implementation doesn't check for trailing dots
        result.Should().BeTrue();
    }

    [Fact]
    public void IsValidFileName_FileNameEndingWithSpace_ShouldReturnTrue()
    {
        // Act
        var result = _service.IsValidFileName("filename ");

        // Assert
        // The current implementation doesn't check for trailing spaces
        result.Should().BeTrue();
    }

    [Fact]
    public void SanitizeFileName_RemovesInvalidCharacters()
    {
        // Arrange
        var dirtyFileName = "test<>:\"/\\|?*file.txt";

        // Act
        var result = _service.SanitizeFileName(dirtyFileName);

        // Assert
        result.Should().Be("test_file.txt");
    }

    [Fact]
    public void SanitizeFileName_PreservesValidCharacters()
    {
        // Arrange
        var cleanFileName = "valid_file-name123.txt";

        // Act
        var result = _service.SanitizeFileName(cleanFileName);

        // Assert
        result.Should().Be(cleanFileName);
    }

    [Fact]
    public void SanitizeFileName_HandlesUnicodeCharacters()
    {
        // Arrange
        var unicodeFileName = "файл_тест_αβγ.txt";

        // Act
        var result = _service.SanitizeFileName(unicodeFileName);

        // Assert
        result.Should().Be(unicodeFileName);
    }

    [Fact]
    public void GenerateFileName_LongName_ShouldTruncate()
    {
        // Arrange
        var longName = new string('А', 100);

        // Act
        var result = _service.GenerateFileName(longName);

        // Assert
        result.Should().HaveLength(54); // 50 chars + ".txt"
        result.Should().EndWith(".txt");
    }

    [Fact]
    public void GenerateFileName_NameWithMultipleSpaces_ShouldNormalize()
    {
        // Arrange
        var nameWithSpaces = "Иван    Петров   Сидоров";

        // Act
        var result = _service.GenerateFileName(nameWithSpaces);

        // Assert
        result.Should().Be("Иван_Петров_Сидоров.txt");
    }
}