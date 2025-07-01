using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SurveyApp.Application.Factories;
using SurveyApp.Application.Processors;
using SurveyApp.Application.Services;
using SurveyApp.Application.Validators;
using SurveyApp.Application.Tests.TestHelpers;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Tests.Integration;

public class ArchitectureIntegrationTests
{
    private readonly TestSurveyBuilderFactory _builderFactory;

    public ArchitectureIntegrationTests()
    {
        var dateTimeProvider = new TestDateTimeProvider { Today = DateTime.Today };
        var settings = Options.Create(new SurveySettings
        {
            DateFormat = "dd.MM.yyyy",
            MinAge = 0,
            MaxAge = 120
        });

        var dateValidator = new DateValidationStrategy(dateTimeProvider, settings);
        var surveyFactory = new SurveyFactory();
        
        var processors = new List<IQuestionProcessor>
        {
            new TextQuestionProcessor(),
            new DateQuestionProcessor(dateValidator),
            new SelectQuestionProcessor(),
            new PhoneQuestionProcessor(),
            new NumberQuestionProcessor()
        };

        // Создаем простую версию SurveyBuilderFactory для тестов
        _builderFactory = new TestSurveyBuilderFactory(surveyFactory, processors);
    }

    [Fact]
    public void ArchitectureComponents_ShouldBeInitialized()
    {
        // Act & Assert
        _builderFactory.Should().NotBeNull();

        var surveyBuilder = _builderFactory.CreateSurveyBuilder("developer");
        surveyBuilder.Should().NotBeNull();
        surveyBuilder.GetQuestionCount().Should().Be(5);
    }

    [Fact]
    public void FullSurveyWorkflow_ShouldWorkEndToEnd()
    {
        // Arrange
        var surveyBuilder = _builderFactory.CreateSurveyBuilder("developer");

        // Act & Assert - Проверяем, что анкета создается корректно
        surveyBuilder.Should().NotBeNull();
        surveyBuilder.GetQuestionCount().Should().Be(5);

        // Тестируем заполнение каждого вопроса
        // Вопрос 0: Имя (Text)
        var nameResult = surveyBuilder.SetAnswer(0, "Иван Петров");
        nameResult.IsValid.Should().BeTrue();
        surveyBuilder.HasAnswer(0).Should().BeTrue();
        surveyBuilder.GetCurrentAnswer(0).Should().Be("Иван Петров");

        // Вопрос 1: Дата рождения (Date)
        var dateResult = surveyBuilder.SetAnswer(1, "15.06.1990");
        dateResult.IsValid.Should().BeTrue();
        surveyBuilder.HasAnswer(1).Should().BeTrue();

        // Вопрос 2: Язык программирования (Select)
        var langResult = surveyBuilder.SetAnswer(2, "CSharp");
        langResult.IsValid.Should().BeTrue();
        surveyBuilder.HasAnswer(2).Should().BeTrue();

        // Вопрос 3: Опыт (Number)
        var expResult = surveyBuilder.SetAnswer(3, "5");
        expResult.IsValid.Should().BeTrue();
        surveyBuilder.HasAnswer(3).Should().BeTrue();

        // Вопрос 4: Телефон (Phone) - необязательный
        var phoneResult = surveyBuilder.SetAnswer(4, "+7(999)123-45-67");
        phoneResult.IsValid.Should().BeTrue();
        surveyBuilder.HasAnswer(4).Should().BeTrue();

        // Проверяем прогресс
        var (completed, total, answeredFields, missingFields) = surveyBuilder.GetProgress();
        completed.Should().Be(5);
        total.Should().Be(5);
        missingFields.Should().BeEmpty();

        // Проверяем валидацию полноты
        var completenessResult = surveyBuilder.ValidateCompleteness();
        completenessResult.IsValid.Should().BeTrue();

        // Строим финальную анкету
        var survey = surveyBuilder.BuildSurvey();
        survey.Should().NotBeNull();
        survey.Answers.Should().HaveCount(5);
        survey.Answers.Should().ContainKey("Введите ваше имя");
        survey.Answers.Should().ContainKey("Дата рождения");
        survey.Answers.Should().ContainKey("Язык программирования");
        survey.Answers.Should().ContainKey("Опыт работы (лет)");
        survey.Answers.Should().ContainKey("Номер телефона");
    }

    [Fact]
    public void ValidationFailures_ShouldPreventInvalidAnswers()
    {
        // Arrange
        var surveyBuilder = _builderFactory.CreateSurveyBuilder("developer");

        // Act & Assert - Тестируем различные случаи невалидных ответов
        
        // Пустое имя (обязательное поле)
        var emptyNameResult = surveyBuilder.SetAnswer(0, "");
        emptyNameResult.IsValid.Should().BeFalse();
        emptyNameResult.ErrorMessage.Should().Contain("не может быть пустым");

        // Невалидная дата
        var invalidDateResult = surveyBuilder.SetAnswer(1, "invalid-date");
        invalidDateResult.IsValid.Should().BeFalse();

        // Дата в будущем
        var futureDateResult = surveyBuilder.SetAnswer(1, "01.01.2030");
        futureDateResult.IsValid.Should().BeFalse();

        // Невалидный номер телефона
        var invalidPhoneResult = surveyBuilder.SetAnswer(4, "invalid-phone");
        invalidPhoneResult.IsValid.Should().BeFalse();
        invalidPhoneResult.ErrorMessage.Should().Contain("формат номера телефона");

        // Невалидное число
        var invalidNumberResult = surveyBuilder.SetAnswer(3, "not-a-number");
        invalidNumberResult.IsValid.Should().BeFalse();
        invalidNumberResult.ErrorMessage.Should().Contain("корректное число");

        // Число вне диапазона
        var outOfRangeResult = surveyBuilder.SetAnswer(3, "-5");
        outOfRangeResult.IsValid.Should().BeFalse();
        outOfRangeResult.ErrorMessage.Should().Contain("Минимальное значение");
    }

    [Fact]
    public void SecurityFeatures_ShouldPreventIncompleteSubmission()
    {
        // Arrange
        var surveyBuilder = _builderFactory.CreateSurveyBuilder("developer");

        // Act - Заполняем только часть обязательных полей
        surveyBuilder.SetAnswer(0, "Иван Петров");
        surveyBuilder.SetAnswer(1, "15.06.1990");
        // Пропускаем вопросы 2 и 3

        // Assert - Проверяем, что система не позволяет завершить неполную анкету
        var completenessResult = surveyBuilder.ValidateCompleteness();
        completenessResult.IsValid.Should().BeFalse();
        completenessResult.ErrorMessage.Should().Contain("Язык программирования");
        completenessResult.ErrorMessage.Should().Contain("Опыт работы");

        // Проверяем прогресс
        var (completed, total, answeredFields, missingFields) = surveyBuilder.GetProgress();
        completed.Should().Be(2);
        total.Should().Be(5);
        missingFields.Should().Contain("Язык программирования");
        missingFields.Should().Contain("Опыт работы (лет)");
    }

    [Fact]
    public void QuestionPrompts_ShouldBeFormattedCorrectly()
    {
        // Arrange
        var surveyBuilder = _builderFactory.CreateSurveyBuilder("developer");

        // Act & Assert - Проверяем, что промпты форматируются правильно
        var namePrompt = surveyBuilder.GetQuestionPrompt(0);
        namePrompt.Should().Contain("Введите ваше имя");
        namePrompt.Should().Contain("мин. 2 символов");
        namePrompt.Should().Contain("макс. 50 символов");

        var datePrompt = surveyBuilder.GetQuestionPrompt(1);
        datePrompt.Should().Contain("Дата рождения");
        datePrompt.Should().Contain("дд.мм.гггг");

        var langPrompt = surveyBuilder.GetQuestionPrompt(2);
        langPrompt.Should().Contain("Язык программирования");

        var expPrompt = surveyBuilder.GetQuestionPrompt(3);
        expPrompt.Should().Contain("Опыт работы");
        expPrompt.Should().Contain("мин. 0");
        expPrompt.Should().Contain("макс. 50");

        var phonePrompt = surveyBuilder.GetQuestionPrompt(4);
        phonePrompt.Should().Contain("Номер телефона");
        phonePrompt.Should().Contain("+7(999)999-99-99");
    }

    [Fact]
    public void SurveyFactory_ShouldSupportMultipleSurveyTypes()
    {
        // Arrange
        var factory = new SurveyFactory();

        // Act
        var availableTypes = factory.GetAvailableSurveyTypes();

        // Assert
        availableTypes.Should().Contain("developer");
        // В будущем здесь могут быть другие типы анкет
        
        // Тестируем создание определения анкеты
        var definition = factory.CreateSurveyDefinition("developer");
        definition.Should().NotBeNull();
        definition.SurveyType.Should().Be("developer");
        definition.DisplayName.Should().Be("Анкета разработчика");
        definition.Questions.Should().HaveCount(5);
    }
}

public class TestSurveyBuilderFactory
{
    private readonly ISurveyFactory _surveyFactory;
    private readonly IEnumerable<IQuestionProcessor> _processors;

    public TestSurveyBuilderFactory(ISurveyFactory surveyFactory, IEnumerable<IQuestionProcessor> processors)
    {
        _surveyFactory = surveyFactory;
        _processors = processors;
    }

    public ISurveyBuilder CreateSurveyBuilder(string surveyType)
    {
        var surveyDefinition = _surveyFactory.CreateSurveyDefinition(surveyType);
        return new FlexibleSurveyBuilder(surveyDefinition, _processors);
    }
}