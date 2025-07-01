using SurveyApp.Application.Commands;

namespace SurveyApp.Console.Commands;

public sealed class CommandFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, Type> _commands;

    public CommandFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _commands = new Dictionary<string, Type>
        {
            { "-new_profile", typeof(NewProfileCommand) },
            { "-statistics", typeof(StatisticsCommand) },
            { "-save", typeof(SaveCommand) },
            { "-find", typeof(FindCommand) },
            { "-delete", typeof(DeleteCommand) },
            { "-list", typeof(ListCommand) },
            { "-list_today", typeof(ListTodayCommand) },
            { "-zip", typeof(ZipCommand) },
            { "-help", typeof(HelpCommand) },
            { "-exit", typeof(ExitCommand) },
            { "-goto_question", typeof(GotoQuestionCommand) },
            { "-goto_prev_question", typeof(GotoPrevQuestionCommand) },
            { "-restart_profile", typeof(RestartProfileCommand) }
        };
    }

    public ICommand? CreateCommand(string commandName)
    {
        if (_commands.TryGetValue(commandName, out var commandType))
        {
            return (ICommand?)_serviceProvider.GetService(commandType);
        }
        return null;
    }

    public IEnumerable<string> GetAvailableCommands()
    {
        return _commands.Keys;
    }
}
