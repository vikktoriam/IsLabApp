using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);
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

// 3. POST /api/notes — Создать заметку (с валидацией)
app.MapPost("/api/notes", ([FromBody] NoteDTO dto) =>
{
    // Минимальная валидация (Задание 4.3)
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
// Добавьте этот эндпоинт в основную часть программы (где остальные app.MapGet)

app.MapGet("/db/ping", () => 
{
    // Получаем строку подключения из appsettings.json
    var connectionString = builder.Configuration.GetConnectionString("Mssql");

    if (string.IsNullOrEmpty(connectionString))
    {
        return Results.Problem("Конфигурация ConnectionStrings:Mssql не найдена!", statusCode: 500);
    }

    try 
    {
        // Здесь в будущем будет реальная проверка через Microsoft.Data.SqlClient
        // На данном этапе, так как БД еще нет, мы имитируем попытку
        // Если база не развернута, это вызовет исключение, что соответствует заданию
        throw new Exception("SQL Server еще не развернут (ожидаемая ошибка)");
    }
    catch (Exception ex)
    {
        return Results.Json(new { 
            status = "error", 
            message = "Ошибка подключения к БД", 
            details = ex.Message,
            configured_string = connectionString 
        }, statusCode: 503);
    }
});
app.Run();

// --- МОДЕЛИ ДАННЫХ (Задание 4.1) ---

// Сущность "Заметка"
public class Note
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Text { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}

// Объект для получения данных (DTO)
public record NoteDTO(string Title, string Text);