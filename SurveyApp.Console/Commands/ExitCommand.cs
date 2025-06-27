using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class ExitCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;

    public string Name => "-exit";
    public string Description => "Выйти из приложения";

    public ExitCommand(IConsoleUI consoleUI)
    {
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        _consoleUI.WriteLine("До свидания!");
        Environment.Exit(0);
        return Result.Success();
    }
}