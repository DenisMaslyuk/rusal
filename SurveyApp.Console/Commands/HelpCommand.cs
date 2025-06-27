using SurveyApp.Application.Commands;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console.Commands;

public sealed class HelpCommand : ICommand
{
    private readonly IConsoleUI _consoleUI;

    public string Name => "-help";
    public string Description => "Показать список доступных команд с описанием";

    public HelpCommand(IConsoleUI consoleUI)
    {
        _consoleUI = consoleUI;
    }

    public async Task<Result> ExecuteAsync(string[] args)
    {
        _consoleUI.WriteLine("Список доступных команд:");
        _consoleUI.WriteLine("");
        _consoleUI.WriteLine("-new_profile - Заполнить новую анкету");
        _consoleUI.WriteLine("-statistics - Показать статистику всех заполненных анкет");
        _consoleUI.WriteLine("-save - Сохранить заполненную анкету");
        _consoleUI.WriteLine("-find <Имя файла анкеты> - Найти анкету и показать данные анкеты в консоль");
        _consoleUI.WriteLine("-delete <Имя файла анкеты> - Удалить указанную анкету");
        _consoleUI.WriteLine("-list - Показать список названий файлов всех сохранённых анкет");
        _consoleUI.WriteLine("-list_today - Показать список названий файлов всех сохранённых анкет, созданных сегодня");
        _consoleUI.WriteLine("-zip <Имя файла анкеты> <Путь для сохранения архива> - Запаковать указанную анкету в архив");
        _consoleUI.WriteLine("-help - Показать список доступных команд с описанием");
        _consoleUI.WriteLine("-exit - Выйти из приложения");
        _consoleUI.WriteLine("");
        _consoleUI.WriteLine("Команды доступные только при заполнении анкеты:");
        _consoleUI.WriteLine("-goto_question <Номер вопроса> - Вернуться к указанному вопросу");
        _consoleUI.WriteLine("-goto_prev_question - Вернуться к предыдущему вопросу");
        _consoleUI.WriteLine("-restart_profile - Заполнить анкету заново");

        return Result.Success();
    }
}
