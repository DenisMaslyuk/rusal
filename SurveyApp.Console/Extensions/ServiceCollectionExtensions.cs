using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SurveyApp.Application.Services;
using SurveyApp.Application.Factories;
using SurveyApp.Application.Processors;
using SurveyApp.Core.Interfaces;
using SurveyApp.Core.Models;
using SurveyApp.Infrastructure.Repositories;
using SurveyApp.Infrastructure.Services;
using SurveyApp.Console.Commands;
using SurveyApp.Console.UI;

namespace SurveyApp.Console.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSurveyServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SurveySettings>(configuration.GetSection("SurveySettings"));
        
        services.AddSingleton<IFileNameService, FileNameService>();
        services.AddSingleton<IAppLogger, ConsoleLogger>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddSingleton<ISurveyRepository, FileSurveyRepository>();
        
        services.AddSingleton<ISurveyFactory, SurveyFactory>();
        services.AddScoped<SurveyBuilderFactory>();
        
        services.AddTransient<SurveyApp.Application.Validators.DateValidationStrategy>();
        
        services.AddTransient<IQuestionProcessor, TextQuestionProcessor>();
        services.AddTransient<IQuestionProcessor, DateQuestionProcessor>();
        services.AddTransient<IQuestionProcessor, SelectQuestionProcessor>();
        services.AddTransient<IQuestionProcessor, PhoneQuestionProcessor>();
        services.AddTransient<IQuestionProcessor, NumberQuestionProcessor>();
        
        services.AddTransient<SurveyApp.Application.Services.SurveyQuestionService>();
        services.AddTransient<SurveyApp.Application.Validators.FullNameValidationStrategy>();
        services.AddTransient<SurveyApp.Application.Validators.ExperienceValidationStrategy>();
        services.AddTransient<SurveyApp.Application.Validators.PhoneValidationStrategy>();
        
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
        services.AddTransient<GotoQuestionCommand>();
        services.AddTransient<GotoPrevQuestionCommand>();
        services.AddTransient<RestartProfileCommand>();
        
        services.AddSingleton<IConsoleUI, ConsoleUI>();
        services.AddSingleton<ApplicationContext>();
        services.AddSingleton<CommandFactory>();
        
        return services;
    }
}