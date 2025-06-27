@echo off
:: Скрипт для сборки и запуска приложения Survey App на Windows

echo === Survey App Build Script ===

:: Проверка наличия .NET SDK
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ .NET SDK не найден!
    echo Установите .NET 8 SDK с https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ✅ .NET SDK найден
dotnet --version

:: Переход в директорию скрипта
cd /d "%~dp0"

echo 📁 Текущая директория: %cd%

:: Очистка предыдущих сборок
echo 🧹 Очистка предыдущих сборок...
dotnet clean

:: Восстановление зависимостей
echo 📦 Восстановление зависимостей...
dotnet restore

:: Сборка проекта
echo 🔨 Сборка проекта...
dotnet build SurveyApp.sln --configuration Release

if %errorlevel% equ 0 (
    echo ✅ Сборка завершена успешно!
    echo.
    echo 🚀 Запуск приложения...
    echo Для выхода из приложения используйте команду: -exit
    echo.
    dotnet run --project SurveyApp.Console --configuration Release
) else (
    echo ❌ Ошибка при сборке проекта!
    pause
    exit /b 1
)