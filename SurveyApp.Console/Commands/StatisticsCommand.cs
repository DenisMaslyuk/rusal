using SurveyApp.Application.Commands;
using SurveyApp.Application.Services;
using SurveyApp.Core.Common;
using SurveyApp.Core.Enums;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class StatisticsCommand : ICommand
{
    private readonly IStatisticsService _statisticsService;
    private readonly IConsoleUI _consoleUI;

    public string Name => "-statistics";
    public string Description => "Показать статистику всех заполненных анкет";

    public StatisticsCommand(IStatisticsService statisticsService, IConsoleUI consoleUI)
    {
        _statisticsService = statisticsService;
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        var result = await _statisticsService.CalculateStatisticsAsync().ConfigureAwait(false);
        
        if (!result.IsSuccess)
        {
            _consoleUI.ShowError(result.Error!);
            return Result.Failure(result.Error!);
        }

        var statistics = result.Value!;
        
        _consoleUI.WriteLine($"1. Средний возраст всех опрошенных: {statistics.AverageAge} {GetAgeWord(statistics.AverageAge)}");
        _consoleUI.WriteLine($"2. Самый популярный язык программирования: {statistics.MostPopularLanguage.ToDisplayString()}");
        _consoleUI.WriteLine($"3. Самый опытный программист: {statistics.MostExperiencedProgrammer}");

        return Result.Success();
    }

    private static string GetAgeWord(int age)
    {
        if (age % 10 == 1 && age % 100 != 11)
            return "год";
        if (age % 10 >= 2 && age % 10 <= 4 && (age % 100 < 10 || age % 100 >= 20))
            return "года";
        return "лет";
    }
}