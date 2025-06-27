using Microsoft.Extensions.DependencyInjection;
using SurveyApp.Application.Services;
using SurveyApp.Core.Interfaces;
using SurveyApp.Infrastructure.Repositories;
using SurveyApp.Infrastructure.Services;
using SurveyApp.Console.Commands;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSurveyServices(this IServiceCollection services)
    {
        services.AddSingleton<ISurveyRepository, FileSurveyRepository>();
        
        services.AddScoped<IArchiveService, ZipArchiveService>();
        services.AddScoped<IStatisticsService, StatisticsService>();
        
        services.AddTransient<NewProfileCommand>();
        services.AddTransient<StatisticsCommand>();
        services.AddTransient<SaveCommand>();
        services.AddTransient<FindCommand>();
        services.AddTransient<DeleteCommand>();
        services.AddTransient<ListCommand>();
        services.AddTransient<ListTodayCommand>();
        services.AddTransient<ZipCommand>();
        services.AddTransient<HelpCommand>();
        services.AddTransient<ExitCommand>();
        
        services.AddSingleton<IConsoleUI, ConsoleUI>();
        services.AddSingleton<ApplicationContext>();
        services.AddSingleton<CommandFactory>();
        
        return services;
    }
}