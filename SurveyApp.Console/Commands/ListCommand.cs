using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class ListCommand : ICommand
{
    private readonly ISurveyRepository _repository;
    private readonly IConsoleUI _consoleUI;

    public string Name => "-list";
    public string Description => "Показать список названий файлов всех сохранённых анкет";

    public ListCommand(ISurveyRepository repository, IConsoleUI consoleUI)
    {
        _repository = repository;
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        try
        {
            var fileNames = await _repository.GetFileNamesAsync().ConfigureAwait(false);
            var fileList = fileNames.ToList();
            
            if (!fileList.Any())
            {
                _consoleUI.WriteLine("Нет сохранённых анкет");
                return Result.Success();
            }

            _consoleUI.WriteLine("Список всех сохранённых анкет:");
            foreach (var fileName in fileList)
            {
                _consoleUI.WriteLine($"- {fileName}");
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _consoleUI.ShowError($"Ошибка при получении списка анкет: {ex.Message}");
            return Result.Failure(ex.Message);
        }
    }
}