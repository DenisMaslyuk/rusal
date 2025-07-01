using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Services;

public sealed class FlexibleSurveyBuilder : ISurveyBuilder
{
    private readonly ISurveyDefinition _surveyDefinition;
    private readonly Dictionary<QuestionType, IQuestionProcessor> _questionProcessors;
    private readonly Dictionary<int, string> _answers = new();

    public FlexibleSurveyBuilder(ISurveyDefinition surveyDefinition, IEnumerable<IQuestionProcessor> questionProcessors)
    {
        _surveyDefinition = surveyDefinition;
        _questionProcessors = questionProcessors.ToDictionary(p => p.SupportedType);
    }

    public Survey BuildSurvey()
    {
        var answers = new Dictionary<string, string>();
        
        foreach (var question in _surveyDefinition.Questions)
        {
            if (_answers.TryGetValue(question.Index, out var answer))
            {
                var processor = GetProcessor(question.Type);
                answers[question.Prompt] = processor.FormatAnswer(answer, question);
            }
        }

        return new Survey
        {
            Answers = answers,
            CreatedAt = DateTime.Now
        };
    }

    public ValidationResult SetAnswer(int questionIndex, string answer)
    {
        var question = GetQuestion(questionIndex);
        var processor = GetProcessor(question.Type);
        var validationResult = processor.ValidateAnswer(answer, question);
        
        if (validationResult.IsValid)
        {
            _answers[questionIndex] = answer;
        }
        
        return validationResult;
    }

    public ValidationResult ValidateCompleteness()
    {
        var missingRequired = new List<string>();
        
        foreach (var question in _surveyDefinition.Questions.Where(q => q.IsRequired))
        {
            if (!_answers.ContainsKey(question.Index) || string.IsNullOrWhiteSpace(_answers[question.Index]))
            {
                missingRequired.Add(question.Prompt);
            }
        }

        return missingRequired.Count == 0 
            ? ValidationResult.Success() 
            : ValidationResult.Failure($"Не заполнены обязательные поля: {string.Join(", ", missingRequired)}");
    }

    public (int completed, int total, List<string> answeredFields, List<string> missingFields) GetProgress()
    {
        var total = _surveyDefinition.Questions.Count;
        var answeredFields = new List<string>();
        var missingFields = new List<string>();
        
        foreach (var question in _surveyDefinition.Questions)
        {
            if (_answers.ContainsKey(question.Index) && !string.IsNullOrWhiteSpace(_answers[question.Index]))
            {
                answeredFields.Add(question.Prompt);
            }
            else
            {
                missingFields.Add(question.Prompt);
            }
        }

        return (answeredFields.Count, total, answeredFields, missingFields);
    }

    public string GetCurrentAnswer(int questionIndex)
    {
        return _answers.TryGetValue(questionIndex, out var answer) ? answer : string.Empty;
    }

    public bool HasAnswer(int questionIndex)
    {
        return _answers.ContainsKey(questionIndex) && !string.IsNullOrWhiteSpace(_answers[questionIndex]);
    }

    public string GetQuestionPrompt(int questionIndex)
    {
        var question = GetQuestion(questionIndex);
        var processor = GetProcessor(question.Type);
        return processor.GetPrompt(question);
    }

    public int GetQuestionCount()
    {
        return _surveyDefinition.Questions.Count;
    }

    public ValidationResult ValidateAnswer(int questionIndex, string answer)
    {
        var question = GetQuestion(questionIndex);
        var processor = GetProcessor(question.Type);
        return processor.ValidateAnswer(answer, question);
    }

    private IQuestionDefinition GetQuestion(int index)
    {
        var question = _surveyDefinition.Questions.FirstOrDefault(q => q.Index == index);
        return question ?? throw new ArgumentException($"Вопрос с индексом {index} не найден");
    }

    private IQuestionProcessor GetProcessor(QuestionType questionType)
    {
        return _questionProcessors.TryGetValue(questionType, out var processor) 
            ? processor 
            : throw new InvalidOperationException($"Обработчик для типа вопроса {questionType} не найден");
    }
}