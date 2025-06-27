using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.UI;

public sealed class ConsoleUI : IConsoleUI
{
    public void WriteLine(string message)
    {
        System.Console.WriteLine(message);
    }

    public void Write(string message)
    {
        System.Console.Write(message);
    }

    public string ReadLine()
    {
        return System.Console.ReadLine() ?? string.Empty;
    }

    public void Clear()
    {
        System.Console.Clear();
    }

    public void ShowError(string message)
    {
        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        WriteLine($"Ошибка: {message}");
        System.Console.ForegroundColor = originalColor;
    }

    public void ShowSuccess(string message)
    {
        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Green;
        WriteLine(message);
        System.Console.ForegroundColor = originalColor;
    }
}