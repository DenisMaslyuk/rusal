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
    private static readonly Dictionary<string, ProgrammingLanguage> DisplayNameToEnum = new()
    {
        ["PHP"] = ProgrammingLanguage.PHP,
        ["JavaScript"] = ProgrammingLanguage.JavaScript,
        ["C"] = ProgrammingLanguage.C,
        ["C++"] = ProgrammingLanguage.CPlusPlus,
        ["Java"] = ProgrammingLanguage.Java,
        ["C#"] = ProgrammingLanguage.CSharp,
        ["Python"] = ProgrammingLanguage.Python,
        ["Ruby"] = ProgrammingLanguage.Ruby
    };

    private static readonly Dictionary<ProgrammingLanguage, string> EnumToDisplayName =
        DisplayNameToEnum.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    private static readonly string[] AllDisplayNames = DisplayNameToEnum.Keys.ToArray();

    public static bool TryParse(string value, out ProgrammingLanguage language)
    {
        return DisplayNameToEnum.TryGetValue(value, out language);
    }

    public static string ToDisplayString(this ProgrammingLanguage language)
    {
        return EnumToDisplayName.TryGetValue(language, out var displayName)
            ? displayName
            : language.ToString();
    }

    /// <summary>
    /// Возвращает все отображаемые имена языков программирования
    /// </summary>
    /// <returns>Массив отображаемых имен</returns>
    public static string[] GetAllDisplayNames()
    {
        return AllDisplayNames;
    }
}
