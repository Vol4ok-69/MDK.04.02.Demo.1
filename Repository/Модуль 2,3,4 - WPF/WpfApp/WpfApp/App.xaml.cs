using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Text.Json;
using System.Windows;
using WpfApp.Models;

namespace WpfApp;

public partial class App : Application
{
    private const string ConnectionFileName = "connection.json";

    public static DataBaseContext CreateDbContext()
    {
        string filePath = Path.Combine(
            AppContext.BaseDirectory,
            ConnectionFileName);

        if (!File.Exists(filePath))
        {
            throw new InvalidOperationException(
                $"Не найден файл конфигурации базы данных: {ConnectionFileName}");
        }

        try
        {
            string json = File.ReadAllText(filePath);

            using JsonDocument document =
                JsonDocument.Parse(json);

            if (!document.RootElement.TryGetProperty(
                    "ConnectionString",
                    out JsonElement connectionProperty))
            {
                throw new InvalidOperationException(
                    "В файле connection.json отсутствует параметр ConnectionString.");
            }

            string? connectionString =
                connectionProperty.GetString();

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Строка подключения к базе данных не указана.");
            }

            var options =
                new DbContextOptionsBuilder<DataBaseContext>()
                    .UseNpgsql(connectionString)
                    .Options;

            return new DataBaseContext(options);
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Файл connection.json содержит некорректный JSON.");
        }
    }
}