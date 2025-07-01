using FluentAssertions;
using Moq;
using SurveyApp.Console.Commands;
using SurveyApp.Console.UI;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Console.Tests.Commands;

public class GotoQuestionCommandTests
{
    private readonly Mock<IConsoleUI> _mockConsoleUI;
    private readonly ApplicationContext _context;
    private readonly Mock<ISurveyBuilder> _mockSurveyBuilder;
    private readonly GotoQuestionCommand _command;

    public GotoQuestionCommandTests()
    {
        _mockConsoleUI = new Mock<IConsoleUI>();
        _context = new ApplicationContext();
        _mockSurveyBuilder = new Mock<ISurveyBuilder>();
        _command = new GotoQuestionCommand(_mockConsoleUI.Object, _context, _mockSurveyBuilder.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNotInSurveyMode_ShouldReturnFailure()
    {
        // Arrange
        _context.IsInSurveyMode = false;
        var args = new[] { "-goto_question", "3" };

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Не в режиме анкетирования");
        _mockConsoleUI.Verify(ui => ui.ShowError("Команда доступна только во время заполнения анкеты"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithMissingQuestionNumber_ShouldReturnFailure()
    {
        // Arrange
        _context.IsInSurveyMode = true;
        var args = new[] { "-goto_question" };

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Не указан номер вопроса");
        _mockConsoleUI.Verify(ui => ui.ShowError("Укажите номер вопроса. Пример: -goto_question 3"), Times.Once);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("1.5")]
    [InlineData("")]
    public async Task ExecuteAsync_WithInvalidQuestionNumber_ShouldReturnFailure(string invalidNumber)
    {
        // Arrange
        _context.IsInSurveyMode = true;
        var args = new[] { "-goto_question", invalidNumber };

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Неверный номер вопроса");
        _mockConsoleUI.Verify(ui => ui.ShowError("Номер вопроса должен быть числом"), Times.Once);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("6")]
    [InlineData("-1")]
    [InlineData("100")]
    public async Task ExecuteAsync_WithQuestionNumberOutOfRange_ShouldReturnFailure(string outOfRangeNumber)
    {
        // Arrange
        _context.IsInSurveyMode = true;
        var args = new[] { "-goto_question", outOfRangeNumber };

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Номер вопроса вне диапазона");
        _mockConsoleUI.Verify(ui => ui.ShowError("Номер вопроса должен быть от 1 до 5"), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithBackwardNavigation_ShouldNavigateWithoutWarning()
    {
        // Arrange
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 3; // На 4-м вопросе
        var args = new[] { "-goto_question", "2" }; // Переход к 2-му

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.CurrentQuestionIndex.Should().Be(1); // Индекс 1 = вопрос 2
        _mockConsoleUI.Verify(ui => ui.ShowSuccess("Переход к вопросу 2"), Times.Once);
        _mockSurveyBuilder.Verify(sb => sb.GetProgress(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithForwardNavigationAndNoMissingFields_ShouldNavigateWithoutWarning()
    {
        // Arrange
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 1; // На 2-м вопросе
        var args = new[] { "-goto_question", "4" }; // Переход к 4-му

        var answeredFields = new List<string> { "ФИО", "Дата рождения", "Язык", "Опыт", "Телефон" };
        var missingFields = new List<string>(); // Нет пропущенных полей

        _mockSurveyBuilder.Setup(sb => sb.GetProgress())
            .Returns((answeredFields.Count, 5, answeredFields, missingFields));

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.CurrentQuestionIndex.Should().Be(3); // Индекс 3 = вопрос 4
        _mockConsoleUI.Verify(ui => ui.ShowSuccess("Переход к вопросу 4"), Times.Once);
        _mockSurveyBuilder.Verify(sb => sb.GetProgress(), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WithForwardNavigationAndMissingFields_WhenUserConfirms_ShouldNavigate()
    {
        // Arrange
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 0; // На 1-м вопросе
        var args = new[] { "-goto_question", "3" }; // Переход к 3-му

        var answeredFields = new List<string> { "Язык", "Опыт" };
        var missingFields = new List<string> { "ФИО", "Дата рождения", "Мобильный телефон" };

        _mockSurveyBuilder.Setup(sb => sb.GetProgress())
            .Returns((2, 5, answeredFields, missingFields));

        _mockConsoleUI.Setup(ui => ui.ReadLine()).Returns("y"); // Пользователь подтверждает

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.CurrentQuestionIndex.Should().Be(2); // Индекс 2 = вопрос 3
        _mockConsoleUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("Предупреждение"))), Times.Once);
        _mockConsoleUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("ФИО"))), Times.Once);
        _mockConsoleUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("Дата рождения"))), Times.Once);
        _mockConsoleUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("Мобильный телефон"))), Times.Once);
        _mockConsoleUI.Verify(ui => ui.WriteLine("Продолжить переход? (y/n):"), Times.Once);
        _mockConsoleUI.Verify(ui => ui.ShowSuccess("Переход к вопросу 3"), Times.Once);
    }

    [Theory]
    [InlineData("n")]
    [InlineData("N")]
    [InlineData("no")]
    [InlineData("нет")]
    [InlineData("")]
    public async Task ExecuteAsync_WithForwardNavigationAndMissingFields_WhenUserDeclines_ShouldCancelNavigation(string userInput)
    {
        // Arrange
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 0; // На 1-м вопросе
        var args = new[] { "-goto_question", "5" }; // Переход к 5-му

        var answeredFields = new List<string> { "Язык", "Опыт" };
        var missingFields = new List<string> { "ФИО", "Дата рождения", "Мобильный телефон" };

        _mockSurveyBuilder.Setup(sb => sb.GetProgress())
            .Returns((2, 5, answeredFields, missingFields));

        _mockConsoleUI.Setup(ui => ui.ReadLine()).Returns(userInput); // Пользователь отклоняет

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.CurrentQuestionIndex.Should().Be(0); // Остается на том же вопросе
        _mockConsoleUI.Verify(ui => ui.WriteLine(It.Is<string>(s => s.Contains("Предупреждение"))), Times.Once);
        _mockConsoleUI.Verify(ui => ui.WriteLine("Переход отменен"), Times.Once);
        _mockConsoleUI.Verify(ui => ui.ShowSuccess(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithSameQuestionNumber_ShouldNavigateWithoutWarning()
    {
        // Arrange
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 2; // На 3-м вопросе
        var args = new[] { "-goto_question", "3" }; // "Переход" к тому же 3-му

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.CurrentQuestionIndex.Should().Be(2); // Остается на том же индексе
        _mockConsoleUI.Verify(ui => ui.ShowSuccess("Переход к вопросу 3"), Times.Once);
        _mockSurveyBuilder.Verify(sb => sb.GetProgress(), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidQuestionNumber_ShouldUpdateCurrentQuestionIndex()
    {
        // Arrange
        _context.IsInSurveyMode = true;
        _context.CurrentQuestionIndex = 0;
        var args = new[] { "-goto_question", "4" };

        var answeredFields = new List<string> { "ФИО", "Дата рождения", "Язык", "Опыт", "Телефон" };
        var missingFields = new List<string>();

        _mockSurveyBuilder.Setup(sb => sb.GetProgress())
            .Returns((answeredFields.Count, 5, answeredFields, missingFields));

        // Act
        var result = await _command.ExecuteAsync(args);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _context.CurrentQuestionIndex.Should().Be(3); // Вопрос 4 = индекс 3
    }

    [Fact]
    public void Name_ShouldReturnCorrectCommandName()
    {
        // Assert
        _command.Name.Should().Be("-goto_question");
    }

    [Fact]
    public void Description_ShouldReturnCorrectDescription()
    {
        // Assert
        _command.Description.Should().Be("Перейти к указанному вопросу (доступно только во время заполнения анкеты)");
    }
}