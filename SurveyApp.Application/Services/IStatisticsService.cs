using SurveyApp.Core.Common;
using SurveyApp.Core.Models;

namespace SurveyApp.Application.Services;

public interface IStatisticsService
{
    Task<Result<SurveyStatistics>> CalculateStatisticsAsync();
}