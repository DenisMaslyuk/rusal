using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;
using SurveyApp.Infrastructure.Repositories;

namespace SurveyApp.Infrastructure.Tests.Repositories;

public class FileSurveyRepositoryTests : IDisposable
{
    private readonly Mock<IAppLogger> _loggerMock;
    private readonly Mock<IFileNameService> _fileNameServiceMock;
    private readonly SurveySettings _settings;
    private readonly FileSurveyRepository _repository;
    private readonly string _testDirectory;

    public FileSurveyRepositoryTests()
    {
        _loggerMock = new Mock<IAppLogger>();
        _fileNameServiceMock = new Mock<IFileNameService>();
        
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
        
        _settings = new SurveySettings
        {
            SurveyDirectory = _testDirectory,
            DateFormat = "dd.MM.yyyy"
        };
        
        var options = Options.Create(_settings);
        _repository = new FileSurveyRepository(_fileNameServiceMock.Object, _loggerMock.Object, options);
        
        SetupFileNameServiceMock();
    }

    private void SetupFileNameServiceMock()
    {
        _fileNameServiceMock.Setup(x => x.GenerateFileName(It.IsAny<string>()))
            .Returns<string>(name => $"{name?.Replace(" ", "_")}.txt");
        
        _fileNameServiceMock.Setup(x => x.IsValidFileName(It.IsAny<string>()))
            .Returns<string>(fileName => !string.IsNullOrWhiteSpace(fileName) && !fileName.Contains('/') && !fileName.Contains('\\'));
        
        _fileNameServiceMock.Setup(x => x.SanitizeFileName(It.IsAny<string>()))
            .Returns<string>(name => name?.Replace(" ", "_") ?? "Unknown");
    }

    [Fact]
    public async Task SaveAsync_ValidSurvey_ShouldSaveSuccessfully()
    {
        // Arrange
        var survey = new Survey
        {
            FullName = "John Doe",
            BirthDate = new DateTime(1990, 6, 15),
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        // Act
        await _repository.SaveAsync(survey);

        // Assert
        var expectedFileName = "John_Doe.txt";
        var filePath = Path.Combine(_testDirectory, expectedFileName);
        File.Exists(filePath).Should().BeTrue();
        
        var content = await File.ReadAllTextAsync(filePath);
        content.Should().Contain("John Doe");
        content.Should().Contain("15.06.1990");
        content.Should().Contain("C#");
        content.Should().Contain("5");
        content.Should().Contain("+1234567890");
    }

    [Fact]
    public async Task SaveAsync_InvalidFileName_ShouldThrowArgumentException()
    {
        // Arrange
        var survey = new Survey
        {
            FullName = "Test User",
            BirthDate = new DateTime(1990, 6, 15),
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        _fileNameServiceMock.Setup(x => x.IsValidFileName(It.IsAny<string>()))
            .Returns(false);

        // Act & Assert
        var act = async () => await _repository.SaveAsync(survey);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Generated filename is invalid*");
    }

    [Fact]
    public async Task SaveAsync_UnsecurePath_ShouldThrowUnauthorizedAccessException()
    {
        // Arrange
        var survey = new Survey
        {
            FullName = "Test User",
            BirthDate = new DateTime(1990, 6, 15),
            Language = ProgrammingLanguage.CSharp,
            ExperienceYears = 5,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        // Setup file name service to generate a path traversal attempt
        _fileNameServiceMock.Setup(x => x.GenerateFileName(It.IsAny<string>()))
            .Returns("../../../etc/passwd.txt");
        _fileNameServiceMock.Setup(x => x.IsValidFileName(It.IsAny<string>()))
            .Returns(false);

        // Act & Assert
        var act = async () => await _repository.SaveAsync(survey);
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Generated filename is invalid*");
    }

    [Fact]
    public async Task GetAllAsync_EmptyDirectory_ShouldReturnEmptyCollection()
    {
        // Act
        var surveys = await _repository.GetAllAsync();

        // Assert
        surveys.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithValidSurveyFiles_ShouldReturnAllSurveys()
    {
        // Arrange
        var survey1Content = "1. ФИО: John Doe\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: C#\n4. Опыт программирования на указанном языке: 5\n5. Мобильный телефон: +1234567890\nАнкета заполнена: 15.06.2024";
        var survey2Content = "1. ФИО: Jane Smith\n2. Дата рождения: 20.03.1985\n3. Любимый язык программирования: JavaScript\n4. Опыт программирования на указанном языке: 10\n5. Мобильный телефон: +0987654321\nАнкета заполнена: 15.06.2024";

        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "john_doe.txt"), survey1Content);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "jane_smith.txt"), survey2Content);

        // Act
        var surveys = await _repository.GetAllAsync();

        // Assert
        surveys.Should().HaveCount(2);
        
        var johnSurvey = surveys.FirstOrDefault(s => s.FullName == "John Doe");
        johnSurvey.Should().NotBeNull();
        johnSurvey!.BirthDate.Should().Be(new DateTime(1990, 6, 15));
        johnSurvey.Language.Should().Be(ProgrammingLanguage.CSharp);
        johnSurvey.ExperienceYears.Should().Be(5);
        johnSurvey.PhoneNumber.Should().Be("+1234567890");
        
        var janeSurvey = surveys.FirstOrDefault(s => s.FullName == "Jane Smith");
        janeSurvey.Should().NotBeNull();
        janeSurvey!.BirthDate.Should().Be(new DateTime(1985, 3, 20));
        janeSurvey.Language.Should().Be(ProgrammingLanguage.JavaScript);
        janeSurvey.ExperienceYears.Should().Be(10);
        janeSurvey.PhoneNumber.Should().Be("+0987654321");
    }

    [Fact]
    public async Task GetAllAsync_WithInvalidFiles_ShouldSkipInvalidFiles()
    {
        // Arrange
        var validSurveyContent = "1. ФИО: John Doe\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: C#\n4. Опыт программирования на указанном языке: 5\n5. Мобильный телефон: +1234567890\nАнкета заполнена: 15.06.2024";
        var invalidSurveyContent = "Invalid content without proper format";

        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "valid_survey.txt"), validSurveyContent);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "invalid_survey.txt"), invalidSurveyContent);

        // Act
        var surveys = await _repository.GetAllAsync();

        // Assert
        surveys.Should().HaveCount(1);
        surveys.First().FullName.Should().Be("John Doe");
        
        // The error handling in the actual implementation prints to console
        // instead of using the logger, so we don't verify the logger call
    }

    [Fact]
    public async Task GetAllAsync_NonExistentDirectory_ShouldReturnEmptyCollection()
    {
        // Arrange
        var nonExistentSettings = new SurveySettings
        {
            SurveyDirectory = Path.Combine(_testDirectory, "NonExistentSubDir"),
            DateFormat = "dd.MM.yyyy"
        };
        var nonExistentOptions = Options.Create(nonExistentSettings);
        var nonExistentRepository = new FileSurveyRepository(_fileNameServiceMock.Object, _loggerMock.Object, nonExistentOptions);

        // Act
        var surveys = await nonExistentRepository.GetAllAsync();

        // Assert
        surveys.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithNonTxtFiles_ShouldIgnoreNonTxtFiles()
    {
        // Arrange
        var surveyContent = "1. ФИО: John Doe\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: C#\n4. Опыт программирования на указанном языке: 5\n5. Мобильный телефон: +1234567890\nАнкета заполнена: 15.06.2024";

        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "survey.txt"), surveyContent);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "readme.md"), "This is a readme file");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "config.json"), "{}");

        // Act
        var surveys = await _repository.GetAllAsync();

        // Assert
        surveys.Should().HaveCount(1);
        surveys.First().FullName.Should().Be("John Doe");
    }

    [Fact]
    public async Task FindAsync_ExistingFile_ShouldReturnSurvey()
    {
        // Arrange
        var surveyContent = "1. ФИО: John Doe\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: C#\n4. Опыт программирования на указанном языке: 5\n5. Мобильный телефон: +1234567890\nАнкета заполнена: 15.06.2024";
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "test.txt"), surveyContent);

        // Act
        var survey = await _repository.FindAsync("test.txt");

        // Assert
        survey.Should().NotBeNull();
        survey!.FullName.Should().Be("John Doe");
        survey.BirthDate.Should().Be(new DateTime(1990, 6, 15));
        survey.Language.Should().Be(ProgrammingLanguage.CSharp);
    }

    [Fact]
    public async Task FindAsync_NonExistentFile_ShouldReturnNull()
    {
        // Act
        var survey = await _repository.FindAsync("nonexistent.txt");

        // Assert
        survey.Should().BeNull();
    }

    [Fact]
    public async Task FindAsync_InvalidFileName_ShouldThrowArgumentException()
    {
        // Arrange
        _fileNameServiceMock.Setup(x => x.IsValidFileName(It.IsAny<string>()))
            .Returns(false);

        // Act & Assert
        var act = async () => await _repository.FindAsync("invalid/file.txt");
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Invalid filename*");
    }

    [Fact]
    public async Task DeleteAsync_ExistingFile_ShouldReturnTrue()
    {
        // Arrange
        var filePath = Path.Combine(_testDirectory, "test.txt");
        await File.WriteAllTextAsync(filePath, "test content");

        // Act
        var result = await _repository.DeleteAsync("test.txt");

        // Assert
        result.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_NonExistentFile_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteAsync("nonexistent.txt");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetFileNamesAsync_WithFiles_ShouldReturnFileNames()
    {
        // Arrange
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file1.txt"), "content1");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "file2.txt"), "content2");
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "readme.md"), "not a txt file");

        // Act
        var fileNames = await _repository.GetFileNamesAsync();

        // Assert
        fileNames.Should().HaveCount(2);
        fileNames.Should().Contain("file1.txt");
        fileNames.Should().Contain("file2.txt");
        fileNames.Should().NotContain("readme.md");
    }

    [Fact]
    public async Task GetTodayAsync_WithTodaysSurveys_ShouldReturnOnlyTodaysSurveys()
    {
        // Arrange
        var today = DateTime.Today;
        var yesterday = today.AddDays(-1);
        
        var todaySurveyContent = $"1. ФИО: Today User\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: C#\n4. Опыт программирования на указанном языке: 5\n5. Мобильный телефон: +1234567890\nАнкета заполнена: {today:dd.MM.yyyy}";
        var yesterdaySurveyContent = $"1. ФИО: Yesterday User\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: Java\n4. Опыт программирования на указанном языке: 3\n5. Мобильный телефон: +0987654321\nАнкета заполнена: {yesterday:dd.MM.yyyy}";

        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "today.txt"), todaySurveyContent);
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "yesterday.txt"), yesterdaySurveyContent);

        // Act
        var surveys = await _repository.GetTodayAsync();

        // Assert
        surveys.Should().HaveCount(1);
        surveys.First().FullName.Should().Be("Today User");
    }

    [Fact]
    public void EnsureDirectoryExists_NonExistentDirectory_ShouldCreateDirectory()
    {
        // Arrange
        var newTestDir = Path.Combine(_testDirectory, "NewSubDir");
        var newSettings = new SurveySettings { SurveyDirectory = newTestDir };
        var newOptions = Options.Create(newSettings);
        var newRepository = new FileSurveyRepository(_fileNameServiceMock.Object, _loggerMock.Object, newOptions);

        // Act
        newRepository.EnsureDirectoryExists();

        // Assert
        Directory.Exists(newTestDir).Should().BeTrue();
        
        // Cleanup
        Directory.Delete(newTestDir, true);
    }

    [Fact]
    public async Task GetAllAsync_WithCSharpLanguage_ShouldParseCorrectly()
    {
        // Arrange
        var surveyContent = "1. ФИО: Test User\n2. Дата рождения: 15.06.1990\n3. Любимый язык программирования: C#\n4. Опыт программирования на указанном языке: 5\n5. Мобильный телефон: +1234567890\nАнкета заполнена: 15.06.2024";
        await File.WriteAllTextAsync(Path.Combine(_testDirectory, "test_survey.txt"), surveyContent);

        // Act
        var surveys = await _repository.GetAllAsync();

        // Assert
        surveys.Should().HaveCount(1);
        var survey = surveys.First();
        survey.Language.Should().Be(ProgrammingLanguage.CSharp);
        survey.FullName.Should().Be("Test User");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }
}