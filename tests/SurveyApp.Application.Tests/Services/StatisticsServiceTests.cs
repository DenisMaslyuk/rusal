using FluentAssertions;
using Moq;
using SurveyApp.Application.Services;
using SurveyApp.Application.Tests.TestHelpers;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Services;

public class StatisticsServiceTests
{
    private readonly Mock<ISurveyRepository> _repositoryMock;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly StatisticsService _service;

    public StatisticsServiceTests()
    {
        _repositoryMock = new Mock<ISurveyRepository>();
        _dateTimeProvider = new TestDateTimeProvider
        {
            Today = new DateTime(2024, 6, 15)
        };
        _service = new StatisticsService(_repositoryMock.Object, _dateTimeProvider);
    }

    [Fact]
    public async Task CalculateStatisticsAsync_WithEmptyRepository_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(Enumerable.Empty<Survey>());

        // Act
        var result = await _service.CalculateStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("анкет не найдено");
    }

    [Fact]
    public async Task CalculateStatisticsAsync_WithValidSurveys_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var surveys = new[]
        {
            new Survey
            {
                FullName = "John Doe",
                BirthDate = new DateTime(1990, 6, 15), // 34 years old
                Language = ProgrammingLanguage.CSharp,
                ExperienceYears = 5,
                PhoneNumber = "+1234567890",
                CreatedAt = DateTime.Now
            },
            new Survey
            {
                FullName = "Jane Smith",
                BirthDate = new DateTime(1985, 6, 15), // 39 years old
                Language = ProgrammingLanguage.JavaScript,
                ExperienceYears = 10,
                PhoneNumber = "+1234567891",
                CreatedAt = DateTime.Now
            },
            new Survey
            {
                FullName = "Bob Johnson",
                BirthDate = new DateTime(1995, 6, 15), // 29 years old
                Language = ProgrammingLanguage.CSharp,
                ExperienceYears = 15,
                PhoneNumber = "+1234567892",
                CreatedAt = DateTime.Now
            }
        };

        _repositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(surveys);

        // Act
        var result = await _service.CalculateStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        
        var statistics = result.Value!;
        statistics.AverageAge.Should().Be(34); // (34 + 39 + 29) / 3 = 34
        statistics.MostPopularLanguage.Should().Be(ProgrammingLanguage.CSharp); // 2 out of 3
        statistics.MostExperiencedProgrammer.Should().Be("Bob Johnson"); // 15 years
    }

    [Fact]
    public async Task CalculateStatisticsAsync_WithTiedLanguages_ShouldReturnFirstFound()
    {
        // Arrange
        var surveys = new[]
        {
            new Survey
            {
                FullName = "John Doe",
                BirthDate = new DateTime(1990, 6, 15),
                Language = ProgrammingLanguage.CSharp,
                ExperienceYears = 5,
                PhoneNumber = "+1234567890",
                CreatedAt = DateTime.Now
            },
            new Survey
            {
                FullName = "Jane Smith",
                BirthDate = new DateTime(1985, 6, 15),
                Language = ProgrammingLanguage.JavaScript,
                ExperienceYears = 5,
                PhoneNumber = "+1234567891",
                CreatedAt = DateTime.Now
            }
        };

        _repositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(surveys);

        // Act
        var result = await _service.CalculateStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var statistics = result.Value!;
        
        // Should return one of the languages (implementation specific)
        new[] { ProgrammingLanguage.CSharp, ProgrammingLanguage.JavaScript }
            .Should().Contain(statistics.MostPopularLanguage);
    }

    [Fact]
    public async Task CalculateStatisticsAsync_WithTiedExperience_ShouldReturnFirstFound()
    {
        // Arrange
        var surveys = new[]
        {
            new Survey
            {
                FullName = "John Doe",
                BirthDate = new DateTime(1990, 6, 15),
                Language = ProgrammingLanguage.CSharp,
                ExperienceYears = 10,
                PhoneNumber = "+1234567890",
                CreatedAt = DateTime.Now
            },
            new Survey
            {
                FullName = "Jane Smith",
                BirthDate = new DateTime(1985, 6, 15),
                Language = ProgrammingLanguage.JavaScript,
                ExperienceYears = 10,
                PhoneNumber = "+1234567891",
                CreatedAt = DateTime.Now
            }
        };

        _repositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(surveys);

        // Act
        var result = await _service.CalculateStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var statistics = result.Value!;
        
        // Should return one of the names (implementation specific)
        new[] { "John Doe", "Jane Smith" }
            .Should().Contain(statistics.MostExperiencedProgrammer);
    }

    [Fact]
    public async Task CalculateStatisticsAsync_WithRepositoryException_ShouldReturnFailure()
    {
        // Arrange
        _repositoryMock.Setup(x => x.GetAllAsync())
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _service.CalculateStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Ошибка при расчете статистики");
        result.Error.Should().Contain("Database error");
    }

    [Fact]
    public async Task CalculateStatisticsAsync_WithSingleSurvey_ShouldReturnCorrectStatistics()
    {
        // Arrange
        var survey = new Survey
        {
            FullName = "Single User",
            BirthDate = new DateTime(1990, 6, 15), // 34 years old
            Language = ProgrammingLanguage.Python,
            ExperienceYears = 7,
            PhoneNumber = "+1234567890",
            CreatedAt = DateTime.Now
        };

        _repositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new[] { survey });

        // Act
        var result = await _service.CalculateStatisticsAsync();

        // Assert
        result.IsSuccess.Should().BeTrue();
        var statistics = result.Value!;
        
        statistics.AverageAge.Should().Be(34);
        statistics.MostPopularLanguage.Should().Be(ProgrammingLanguage.Python);
        statistics.MostExperiencedProgrammer.Should().Be("Single User");
    }
}