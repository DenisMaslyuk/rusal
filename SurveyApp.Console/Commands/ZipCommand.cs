using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class ZipCommand : ICommand
{
    private readonly IArchiveService _archiveService;
    private readonly IConsoleUI _consoleUI;

    public string Name => "-zip";
    public string Description => "Запаковать указанную анкету в архив и сохранить архив по указанному пути";

    public ZipCommand(IArchiveService archiveService, IConsoleUI consoleUI)
    {
        _archiveService = archiveService;
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        if (args.Length < 2)
        {
            _consoleUI.ShowError("Укажите имя файла анкеты и путь для сохранения архива");
            _consoleUI.WriteLine("Пример: -zip Иванов_Иван_Иванович.txt C:\\Archives");
            return Result.Failure("Недостаточно параметров");
        }

        var fileName = args[0];
        var destinationPath = args[1];
        
        var result = await _archiveService.CreateArchiveAsync(fileName, destinationPath).ConfigureAwait(false);
        
        if (result.IsSuccess)
        {
            _consoleUI.ShowSuccess($"Анкета '{fileName}' успешно заархивирована и сохранена в '{destinationPath}'");
        }
        else
        {
            _consoleUI.ShowError(result.Error!);
        }

        return result;
    }
}