namespace SurveyApp.Core.Interfaces;

public interface IConsoleUI
{
    void WriteLine(string message);
    void Write(string message);
    string ReadLine();
    void Clear();
    void ShowError(string message);
    void ShowSuccess(string message);
}