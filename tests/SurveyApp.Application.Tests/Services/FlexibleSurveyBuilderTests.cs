using FluentAssertions;
using Moq;
using SurveyApp.Application.Services;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Services;

public class FlexibleSurveyBuilderTests
{
    private readonly Mock<ISurveyDefinition> _mockSurveyDefinition;
    private readonly Mock<IQuestionProcessor> _mockTextProcessor;
    private readonly Mock<IQuestionProcessor> _mockDateProcessor;
    private readonly FlexibleSurveyBuilder _builder;

    public FlexibleSurveyBuilderTests()
    {
        _mockSurveyDefinition = new Mock<ISurveyDefinition>();
        _mockTextProcessor = new Mock<IQuestionProcessor>();
        _mockDateProcessor = new Mock<IQuestionProcessor>();

        _mockTextProcessor.Setup(p => p.SupportedType).Returns(QuestionType.Text);
        _mockDateProcessor.Setup(p => p.SupportedType).Returns(QuestionType.Date);

        var questions = new List<IQuestionDefinition>
        {
            new QuestionDefinition { Index = 0, Prompt = "Name", Type = QuestionType.Text, IsRequired = true },
            new QuestionDefinition { Index = 1, Prompt = "Date", Type = QuestionType.Date, IsRequired = true }
        };

        _mockSurveyDefinition.Setup(s => s.Questions).Returns(questions);

        var processors = new List<IQuestionProcessor> { _mockTextProcessor.Object, _mockDateProcessor.Object };
        _builder = new FlexibleSurveyBuilder(_mockSurveyDefinition.Object, processors);
    }

    [Fact]
    public void SetAnswer_ValidAnswer_ShouldReturnSuccess()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _mockTextProcessor.Setup(p => p.FormatAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns("John");

        // Act
        var result = _builder.SetAnswer(0, "John");

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void SetAnswer_InvalidAnswer_ShouldReturnFailure()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Failure("Required"));

        // Act
        var result = _builder.SetAnswer(0, "");

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Be("Required");
    }

    [Fact]
    public void SetAnswer_InvalidQuestionIndex_ShouldThrowException()
    {
        // Act
        var action = () => _builder.SetAnswer(99, "answer");

        // Assert
        action.Should().Throw<ArgumentException>()
            .WithMessage("*Вопрос с индексом 99 не найден*");
    }

    [Fact]
    public void GetQuestionCount_ShouldReturnCorrectCount()
    {
        // Act
        var count = _builder.GetQuestionCount();

        // Assert
        count.Should().Be(2);
    }

    [Fact]
    public void GetQuestionPrompt_ShouldReturnFormattedPrompt()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.GetPrompt(It.IsAny<IQuestionDefinition>()))
            .Returns("Name:");

        // Act
        var prompt = _builder.GetQuestionPrompt(0);

        // Assert
        prompt.Should().Be("Name:");
    }

    [Fact]
    public void HasAnswer_WithAnswer_ShouldReturnTrue()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _builder.SetAnswer(0, "John");

        // Act
        var hasAnswer = _builder.HasAnswer(0);

        // Assert
        hasAnswer.Should().BeTrue();
    }

    [Fact]
    public void HasAnswer_WithoutAnswer_ShouldReturnFalse()
    {
        // Act
        var hasAnswer = _builder.HasAnswer(0);

        // Assert
        hasAnswer.Should().BeFalse();
    }

    [Fact]
    public void GetCurrentAnswer_WithAnswer_ShouldReturnAnswer()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _builder.SetAnswer(0, "John");

        // Act
        var answer = _builder.GetCurrentAnswer(0);

        // Assert
        answer.Should().Be("John");
    }

    [Fact]
    public void GetCurrentAnswer_WithoutAnswer_ShouldReturnEmpty()
    {
        // Act
        var answer = _builder.GetCurrentAnswer(0);

        // Assert
        answer.Should().Be(string.Empty);
    }

    [Fact]
    public void ValidateCompleteness_AllRequiredFilled_ShouldReturnSuccess()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _mockDateProcessor.Setup(p => p.ValidateAnswer("01.01.1990", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());

        _builder.SetAnswer(0, "John");
        _builder.SetAnswer(1, "01.01.1990");

        // Act
        var result = _builder.ValidateCompleteness();

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void ValidateCompleteness_MissingRequired_ShouldReturnFailure()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _builder.SetAnswer(0, "John");
        // Question 1 (Date) is not answered

        // Act
        var result = _builder.ValidateCompleteness();

        // Assert
        result.IsValid.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Date");
    }

    [Fact]
    public void GetProgress_ShouldReturnCorrectCounts()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _builder.SetAnswer(0, "John");

        // Act
        var (completed, total, answeredFields, missingFields) = _builder.GetProgress();

        // Assert
        completed.Should().Be(1);
        total.Should().Be(2);
        answeredFields.Should().Contain("Name");
        missingFields.Should().Contain("Date");
    }

    [Fact]
    public void BuildSurvey_ShouldCreateSurveyWithAnswers()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _mockTextProcessor.Setup(p => p.FormatAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns("John");
        
        _mockDateProcessor.Setup(p => p.ValidateAnswer("01.01.1990", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());
        _mockDateProcessor.Setup(p => p.FormatAnswer("01.01.1990", It.IsAny<IQuestionDefinition>()))
            .Returns("01.01.1990");

        _builder.SetAnswer(0, "John");
        _builder.SetAnswer(1, "01.01.1990");

        // Act
        var survey = _builder.BuildSurvey();

        // Assert
        survey.Should().NotBeNull();
        survey.Answers.Should().ContainKey("Name");
        survey.Answers.Should().ContainKey("Date");
        survey.Answers["Name"].Should().Be("John");
        survey.Answers["Date"].Should().Be("01.01.1990");
        survey.CreatedAt.Should().BeCloseTo(DateTime.Now, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void ValidateAnswer_ShouldUseCorrectProcessor()
    {
        // Arrange
        _mockTextProcessor.Setup(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()))
            .Returns(ValidationResult.Success());

        // Act
        var result = _builder.ValidateAnswer(0, "John");

        // Assert
        result.IsValid.Should().BeTrue();
        _mockTextProcessor.Verify(p => p.ValidateAnswer("John", It.IsAny<IQuestionDefinition>()), Times.Once);
    }
}