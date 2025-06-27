using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.UI;

public abstract class ApplicationState
{
    protected readonly IConsoleUI ConsoleUI;
    protected readonly ApplicationContext Context;

    protected ApplicationState(IConsoleUI consoleUI, ApplicationContext context)
    {
        ConsoleUI = consoleUI;
        Context = context;
    }

    public abstract Task<ApplicationState?> HandleInputAsync(string input);
    public abstract void Display();
}