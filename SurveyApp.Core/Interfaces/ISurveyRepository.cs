using SurveyApp.Core.Models;

namespace SurveyApp.Core.Interfaces;

public interface ISurveyRepository
{
    Task SaveAsync(Survey survey);
    Task<Survey?> FindAsync(string fileName);
    Task<IEnumerable<Survey>> GetAllAsync();
    Task<IEnumerable<Survey>> GetTodayAsync();
    Task<bool> DeleteAsync(string fileName);
    Task<IEnumerable<string>> GetFileNamesAsync();
    void EnsureDirectoryExists();
}