using FluentAssertions;
using SurveyApp.Application.Factories;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Factories;

public class SurveyFactoryTests
{
    private readonly SurveyFactory _factory = new();

    [Fact]
    public void GetAvailableSurveyTypes_ShouldReturnDeveloperType()
    {
        // Act
        var types = _factory.GetAvailableSurveyTypes();

        // Assert
        types.Should().Contain("developer");
        types.Should().HaveCount(1);
    }

    [Fact]
    public void CreateSurveyDefinition_DeveloperType_ShouldReturnValidDefinition()
    {
        // Act
        var definition = _factory.CreateSurveyDefinition("developer");

        // Assert
        definition.Should().NotBeNull();
        definition.SurveyType.Should().Be("developer");
        definition.DisplayName.Should().Be("Анкета разработчика");
        definition.Questions.Should().HaveCount(5);
    }

    [Fact]
    public void CreateSurveyDefinition_DeveloperType_CaseInsensitive_ShouldWork()
    {
        // Act
        var definition = _factory.CreateSurveyDefinition("DEVELOPER");

        // Assert
        definition.Should().NotBeNull();
        definition.SurveyType.Should().Be("developer");
    }

    [Fact]
    public void CreateSurveyDefinition_InvalidType_ShouldThrowException()
    {
        // Act
        var action = () => _factory.CreateSurveyDefinition("invalid");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Неизвестный тип анкеты: invalid*");
    }

    [Fact]
    public void CreateSurveyDefinition_DeveloperType_ShouldHaveCorrectQuestions()
    {
        // Act
        var definition = _factory.CreateSurveyDefinition("developer");

        // Assert
        var questions = definition.Questions.ToList();
        
        // Question 0: Name
        questions[0].Index.Should().Be(0);
        questions[0].Prompt.Should().Be("Введите ваше имя");
        questions[0].Type.Should().Be(QuestionType.Text);
        questions[0].IsRequired.Should().BeTrue();
        questions[0].Options.Should().ContainKey("MinLength");
        questions[0].Options.Should().ContainKey("MaxLength");

        // Question 1: Date of birth
        questions[1].Index.Should().Be(1);
        questions[1].Prompt.Should().Be("Дата рождения");
        questions[1].Type.Should().Be(QuestionType.Date);
        questions[1].IsRequired.Should().BeTrue();

        // Question 2: Programming language
        questions[2].Index.Should().Be(2);
        questions[2].Prompt.Should().Be("Язык программирования");
        questions[2].Type.Should().Be(QuestionType.Select);
        questions[2].IsRequired.Should().BeTrue();
        questions[2].Options.Should().ContainKey("Values");

        // Question 3: Experience
        questions[3].Index.Should().Be(3);
        questions[3].Prompt.Should().Be("Опыт работы (лет)");
        questions[3].Type.Should().Be(QuestionType.Number);
        questions[3].IsRequired.Should().BeTrue();
        questions[3].Options.Should().ContainKey("Min");
        questions[3].Options.Should().ContainKey("Max");

        // Question 4: Phone
        questions[4].Index.Should().Be(4);
        questions[4].Prompt.Should().Be("Номер телефона");
        questions[4].Type.Should().Be(QuestionType.Phone);
        questions[4].IsRequired.Should().BeFalse();
    }
}