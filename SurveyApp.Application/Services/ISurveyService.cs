using SurveyApp.Core.Common;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Services;

public interface ISurveyService
{
    Task<Result<Survey>> CreateNewSurveyAsync();
    Task<Result> SaveSurveyAsync(Survey survey);
    Task<Result<Survey>> FindSurveyAsync(string fileName);
    Task<Result> DeleteSurveyAsync(string fileName);
    Task<Result<IEnumerable<string>>> GetAllFileNamesAsync();
    Task<Result<IEnumerable<string>>> GetTodayFileNamesAsync();
}