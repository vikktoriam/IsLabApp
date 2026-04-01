using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Добавляем сервисы, если нужно (Swagger и т.д.)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Хранилище заметок в оперативной памяти (In-memory)
var notes = new List<Note>();

// --- МАРШРУТЫ API ---

// 1. GET /api/notes — Получить весь список
app.MapGet("/api/notes", () => Results.Ok(notes));

// 2. GET /api/notes/{id} — Получить одну заметку по ID
app.MapGet("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    return note is not null ? Results.Ok(note) : Results.NotFound(new { error = "Заметка не найдена" });
});

// 3. POST /api/notes — Создать заметку
app.MapPost("/api/notes", ([FromBody] NoteDTO dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest(new { error = "Заголовок не может быть пустым" });

    var newNote = new Note
    {
        Id = notes.Count > 0 ? notes.Max(n => n.Id) + 1 : 1,
        Title = dto.Title,
        Text = dto.Text,
        CreatedAt = DateTime.Now
    };

    notes.Add(newNote);
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
});

// 4. DELETE /api/notes/{id} — Удалить заметку
app.MapDelete("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);
    if (note is null) return Results.NotFound(new { error = "Нечего удалять" });

    notes.Remove(note);
    return Results.NoContent();
});

// --- ПРОВЕРКА БАЗЫ ДАННЫХ (Задание 5/8) ---

app.MapGet("/db/ping", async (IConfiguration configuration) =>
{
    var connectionString = configuration.GetConnectionString("Mssql");

    if (string.IsNullOrEmpty(connectionString))
    {
        return Results.Problem("Конфигурация ConnectionStrings:Mssql не найдена!", statusCode: 500);
    }

    try
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        return Results.Ok(new
        {
            status = "ok",
            message = "Соединение установлено",
            details = $"Успешный пинг БД {connection.Database}",
            configured_string = connectionString.Replace("Password=YourStrong!Passw0rd", "Password=***")
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "error",
            message = "Ошибка подключения к БД",
            details = ex.Message,
            configured_string = connectionString
        }, statusCode: 503);
    }
});

// --- СЕРВИСНЫЕ ЭНДПОИНТЫ ---

app.MapGet("/health", () => "Healthy");
app.MapGet("/version", (IConfiguration conf) => conf["App:Version"] ?? "unknown");

app.Run();

// --- МОДЕЛИ ДАННЫХ ---

public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

public record NoteDTO(string Title, string Text);
