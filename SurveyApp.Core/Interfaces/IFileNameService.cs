namespace SurveyApp.Core.Interfaces;

public interface IFileNameService
{
    string GenerateFileName(string fullName);
    bool IsValidFileName(string fileName);
    string SanitizeFileName(string fileName);
}