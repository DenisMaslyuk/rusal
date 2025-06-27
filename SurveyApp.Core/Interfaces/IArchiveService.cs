using SurveyApp.Core.Common;

namespace SurveyApp.Core.Interfaces;

public interface IArchiveService
{
    Task<Result> CreateArchiveAsync(string fileName, string destinationPath);
}