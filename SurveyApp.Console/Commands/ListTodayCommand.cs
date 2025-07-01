using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class ListTodayCommand : ICommand
{
    private readonly ISurveyRepository _repository;
    private readonly IConsoleUI _consoleUI;

    public string Name => "-list_today";
    public string Description => "Показать список названий файлов всех сохранённых анкет, созданных сегодня";

    public ListTodayCommand(ISurveyRepository repository, IConsoleUI consoleUI)
    {
        _repository = repository;
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        try
        {
            var todaySurveys = await _repository.GetTodayAsync().ConfigureAwait(false);
            var surveyList = todaySurveys.ToList();
            
            if (!surveyList.Any())
            {
                _consoleUI.WriteLine("Нет анкет, созданных сегодня");
                return Result.Success();
            }

            _consoleUI.WriteLine("Список анкет, созданных сегодня:");
            foreach (var survey in surveyList)
            {
                var fileName = $"{survey.FullName}_{survey.BirthDate:dd_MM_yyyy}.txt";
                _consoleUI.WriteLine($"- {fileName}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _consoleUI.ShowError($"Ошибка при получении списка сегодняшних анкет: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }
}