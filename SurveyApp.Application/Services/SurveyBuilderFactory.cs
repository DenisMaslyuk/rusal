using SurveyApp.Core.Interfaces;

namespace SurveyApp.Application.Services;

public sealed class SurveyBuilderFactory
{
    private readonly ISurveyFactory _surveyFactory;
    private readonly IServiceProvider _serviceProvider;

    public SurveyBuilderFactory(ISurveyFactory surveyFactory, IServiceProvider serviceProvider)
    {
        _surveyFactory = surveyFactory;
        _serviceProvider = serviceProvider;
    }

    public ISurveyBuilder CreateSurveyBuilder(string surveyType)
    {
        var surveyDefinition = _surveyFactory.CreateSurveyDefinition(surveyType);
        var questionProcessors = (IEnumerable<IQuestionProcessor>)_serviceProvider.GetService(typeof(IEnumerable<IQuestionProcessor>))!;
        
        return new FlexibleSurveyBuilder(surveyDefinition, questionProcessors);
    }
}