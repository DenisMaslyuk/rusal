using Microsoft.Extensions.DependencyInjection;
using SurveyApp.Console.Commands;
using SurveyApp.Console.Extensions;
using SurveyApp.Console.UI;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Console;

class Program
{
    private static IServiceProvider? _serviceProvider;

    static async Task Main(string[] args)
    {
        ConfigureServices();

        var consoleUI = _serviceProvider!.GetRequiredService<IConsoleUI>();
        var commandFactory = _serviceProvider.GetRequiredService<CommandFactory>();
        var context = _serviceProvider.GetRequiredService<ApplicationContext>();
        var repository = _serviceProvider.GetRequiredService<ISurveyRepository>();

        repository.EnsureDirectoryExists();

        consoleUI.WriteLine("=== Система анкетирования ===");
        consoleUI.WriteLine("Добро пожаловать! Введите -help для просмотра доступных команд.");

        while (true)
        {
            consoleUI.WriteLine("");
            consoleUI.Write("Выберите действие: ");

            var input = consoleUI.ReadLine().Trim();

            if (string.IsNullOrWhiteSpace(input))
                continue;

            var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var commandName = parts[0];
            var commandArgs = parts.Length > 1 ? parts[1..] : Array.Empty<string>();

            var command = commandFactory.CreateCommand(commandName);

            if (command == null)
            {
                consoleUI.ShowError($"Неизвестная команда: {commandName}. Введите -help для просмотра доступных команд.");
                continue;
            }

            try
            {
                await command.ExecuteAsync(commandArgs);
            }
            catch (Exception ex)
            {
                consoleUI.ShowError($"Произошла ошибка при выполнении команды: {ex.Message}");
            }
        }
    }

    private static void ConfigureServices()
    {
        var services = new ServiceCollection();

        services.AddSurveyServices();

        _serviceProvider = services.BuildServiceProvider();
    }
}
