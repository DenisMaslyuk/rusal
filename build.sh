#!/bin/bash

# Скрипт для сборки и запуска приложения Survey App

echo "=== Survey App Build Script ==="

# Проверка наличия .NET SDK
if ! command -v dotnet &> /dev/null; then
    echo "❌ .NET SDK не найден!"
    echo "Установите .NET 8 SDK с https://dotnet.microsoft.com/download"
    exit 1
fi

echo "✅ .NET SDK найден: $(dotnet --version)"

# Переход в директорию проекта
cd "$(dirname "$0")"

echo "📁 Текущая директория: $(pwd)"

# Очистка предыдущих сборок
echo "🧹 Очистка предыдущих сборок..."
dotnet clean

# Восстановление зависимостей
echo "📦 Восстановление зависимостей..."
dotnet restore

# Сборка проекта
echo "🔨 Сборка проекта..."
dotnet build SurveyApp.sln --configuration Release

if [ $? -eq 0 ]; then
    echo "✅ Сборка завершена успешно!"
    echo ""
    echo "🚀 Запуск приложения..."
    echo "Для выхода из приложения используйте команду: -exit"
    echo ""
    dotnet run --project SurveyApp.Console --configuration Release
else
    echo "❌ Ошибка при сборке проекта!"
    exit 1
fi