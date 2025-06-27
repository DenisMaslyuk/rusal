using System.IO.Compression;
using SurveyApp.Core.Common;
using SurveyApp.Core.Interfaces;

namespace SurveyApp.Infrastructure.Services;

public sealed class ZipArchiveService : IArchiveService
{
    private readonly ISurveyRepository _surveyRepository;

    public ZipArchiveService(ISurveyRepository surveyRepository)
    {
        _surveyRepository = surveyRepository;
    }

    public async Task<Result> CreateArchiveAsync(string fileName, string destinationPath)
    {
        try
        {
            var surveysDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Анкеты");
            var sourceFilePath = Path.Combine(surveysDirectory, fileName);
            
            if (!File.Exists(sourceFilePath))
            {
                return Result.Failure($"Файл {fileName} не найден");
            }

            var archiveName = Path.GetFileNameWithoutExtension(fileName) + ".zip";
            var fullDestinationPath = Path.Combine(destinationPath, archiveName);

            var destinationDir = Path.GetDirectoryName(fullDestinationPath);
            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            using var archive = ZipFile.Open(fullDestinationPath, ZipArchiveMode.Create);
            archive.CreateEntryFromFile(sourceFilePath, fileName);

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Ошибка при создании архива: {ex.Message}");
        }
    }
}