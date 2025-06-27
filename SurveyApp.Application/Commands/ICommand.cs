using SurveyApp.Core.Common;

namespace SurveyApp.Application.Commands;

public interface ICommand
{
    string Name { get; }
    string Description { get; }
    Task<Result> ExecuteAsync(string[] args);
}