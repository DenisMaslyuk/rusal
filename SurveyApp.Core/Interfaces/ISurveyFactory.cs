namespace SurveyApp.Core.Interfaces;

public interface ISurveyFactory
{
    ISurveyDefinition CreateSurveyDefinition(string surveyType);
    IEnumerable<string> GetAvailableSurveyTypes();
}