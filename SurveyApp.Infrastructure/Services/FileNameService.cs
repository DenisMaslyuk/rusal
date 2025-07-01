using System.Text.RegularExpressions;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Infrastructure.Services;

public sealed class FileNameService : IFileNameService
{
    private static readonly char[] InvalidChars = Path.GetInvalidFileNameChars()
        .Concat(new[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/' })
        .ToArray();

    private static readonly Regex SafeFileNameRegex = new(@"^[a-zA-Z0-9а-яА-Я\s\-_\.]+$", RegexOptions.Compiled);

    public string GenerateFileName(string fullName)
    {
        var firstName = ExtractFirstName(fullName);
        var sanitizedName = SanitizeFileName(firstName);
        return $"{sanitizedName}.txt";
    }

    private string ExtractFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "Unknown";

        var parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        // Если есть части, берем вторую часть как имя (Фамилия Имя Отчество)
        // Если частей меньше 2, берем первую часть
        return parts.Length >= 2 ? parts[1] : parts[0];
    }

    public bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        if (fileName.Length > 260) // Windows MAX_PATH limitation
            return false;

        if (fileName.Contains("..") || fileName.Contains("\\") || fileName.Contains("/"))
            return false;

        return SafeFileNameRegex.IsMatch(fileName);
    }

    public string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Unknown";

        var sanitized = fileName;
        
        foreach (var invalidChar in InvalidChars)
        {
            sanitized = sanitized.Replace(invalidChar, '_');
        }

        sanitized = sanitized.Replace(" ", "_");
        
        sanitized = Regex.Replace(sanitized, @"_{2,}", "_");
        
        sanitized = sanitized.Trim('_');
        
        if (sanitized.Length > 50)
            sanitized = sanitized[..50];

        return string.IsNullOrEmpty(sanitized) ? "Unknown" : sanitized;
    }
}