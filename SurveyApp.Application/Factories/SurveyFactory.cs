using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Factories;

public sealed class SurveyFactory : ISurveyFactory
{
    private static readonly Dictionary<string, ISurveyDefinition> SurveyDefinitions = new()
    {
        ["developer"] = new SurveyDefinition
        {
            SurveyType = "developer",
            DisplayName = "Анкета разработчика",
            Questions = new List<IQuestionDefinition>
            {
                new QuestionDefinition
                {
                    Index = 0,
                    Prompt = "ФИО",
                    Type = QuestionType.Text,
                    IsRequired = true,
                    Options = new Dictionary<string, object>
                    {
                        ["MinLength"] = 2,
                        ["MaxLength"] = 50
                    }
                },
                new QuestionDefinition
                {
                    Index = 1,
                    Prompt = "Дата рождения",
                    Type = QuestionType.Date,
                    IsRequired = true
                },
                new QuestionDefinition
                {
                    Index = 2,
                    Prompt = "Язык программирования",
                    Type = QuestionType.Select,
                    IsRequired = true,
                    Options = new Dictionary<string, object>
                    {
                        ["Values"] = Enum.GetNames<ProgrammingLanguage>()
                    }
                },
                new QuestionDefinition
                {
                    Index = 3,
                    Prompt = "Опыт работы (лет)",
                    Type = QuestionType.Number,
                    IsRequired = true,
                    Options = new Dictionary<string, object>
                    {
                        ["Min"] = 0.0,
                        ["Max"] = 50.0
                    }
                },
                new QuestionDefinition
                {
                    Index = 4,
                    Prompt = "Номер телефона",
                    Type = QuestionType.Phone,
                    IsRequired = false
                }
            }
        }
    };

    public ISurveyDefinition CreateSurveyDefinition(string surveyType)
    {
        if (SurveyDefinitions.TryGetValue(surveyType.ToLowerInvariant(), out var definition))
        {
            return definition;
        }

        throw new ArgumentException($"Неизвестный тип анкеты: {surveyType}");
    }

    public IEnumerable<string> GetAvailableSurveyTypes()
    {
        return SurveyDefinitions.Keys;
    }
}
