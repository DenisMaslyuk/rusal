using FluentAssertions;
using SurveyApp.Application.Processors;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Processors;

public class SelectQuestionProcessorTests
{
    private readonly SelectQuestionProcessor _processor = new();

    [Fact]
    public void SupportedType_ShouldReturnSelect()
    {
        _processor.SupportedType.Should().Be(QuestionType.Select);
    }

    [Fact]
    public void ValidateAnswer_RequiredAndEmpty_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Выберите один из вариантов");
    }

    [Fact]
    public void ValidateAnswer_ValidOption_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> 
            { 
                ["Values"] = new string[] { "Option1", "Option2", "Option3" }
            }
        };

        // Act
        var result = _processor.ValidateAnswer("Option2", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateAnswer_InvalidOption_ShouldReturnFailure()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            IsRequired = true,
            Options = new Dictionary<string, object> 
            { 
                ["Values"] = new string[] { "Option1", "Option2", "Option3" }
            }
        };

        // Act
        var result = _processor.ValidateAnswer("InvalidOption", question);

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Недопустимое значение");
        result.ErrorMessage.Should().Contain("Option1, Option2, Option3");
    }

    [Fact]
    public void ValidateAnswer_WithoutOptions_ShouldReturnSuccess()
    {
        // Arrange
        var question = new QuestionDefinition { IsRequired = true };

        // Act
        var result = _processor.ValidateAnswer("Any value", question);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void FormatAnswer_ShouldTrimWhitespace()
    {
        // Arrange
        var question = new QuestionDefinition();

        // Act
        var result = _processor.FormatAnswer("  Option1  ", question);

        // Assert
        result.Should().Be("Option1");
    }

    [Fact]
    public void GetPrompt_WithoutOptions_ShouldReturnBasicPrompt()
    {
        // Arrange
        var question = new QuestionDefinition { Prompt = "Select option" };

        // Act
        var result = _processor.GetPrompt(question);

        // Assert
        result.Should().Be("Select option:");
    }

    [Fact]
    public void GetPrompt_WithOptions_ShouldIncludeValues()
    {
        // Arrange
        var question = new QuestionDefinition 
        { 
            Prompt = "Select language",
            Options = new Dictionary<string, object> 
            { 
                ["Values"] = new string[] { "C#", "Java", "Python" }
            }
        };

        // Act
        var result = _processor.GetPrompt(question);

        // Assert
        result.Should().Be("Select language (C#/Java/Python):");
    }
}