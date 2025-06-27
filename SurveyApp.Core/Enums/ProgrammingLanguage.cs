namespace SurveyApp.Core.Enums;

public enum ProgrammingLanguage
{
    PHP,
    JavaScript,
    C,
    CPlusPlus,
    Java,
    CSharp,
    Python,
    Ruby
}

public static class ProgrammingLanguageExtensions
{
    private static readonly Dictionary<string, ProgrammingLanguage> LanguageMap = new()
    {
        { "PHP", ProgrammingLanguage.PHP },
        { "JavaScript", ProgrammingLanguage.JavaScript },
        { "C", ProgrammingLanguage.C },
        { "C++", ProgrammingLanguage.CPlusPlus },
        { "Java", ProgrammingLanguage.Java },
        { "C#", ProgrammingLanguage.CSharp },
        { "Python", ProgrammingLanguage.Python },
        { "Ruby", ProgrammingLanguage.Ruby }
    };

    public static bool TryParse(string value, out ProgrammingLanguage language)
    {
        return LanguageMap.TryGetValue(value, out language);
    }

    public static string ToDisplayString(this ProgrammingLanguage language)
    {
        return language switch
        {
            ProgrammingLanguage.CPlusPlus => "C++",
            ProgrammingLanguage.CSharp => "C#",
            _ => language.ToString()
        };
    }

    public static IEnumerable<string> GetAllDisplayNames()
    {
        return LanguageMap.Keys;
    }
}