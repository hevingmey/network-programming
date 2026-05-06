using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;


public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Group { get; set; } = string.Empty;
}


class Program
{
    static List<Student> students = new List<Student>
    {
        new Student { Id = 1,  Name = "Олексій",  Surname = "Петренко",   Group = "КІ-21" },
        new Student { Id = 2,  Name = "Марія",    Surname = "Іваненко",   Group = "КІ-21" },
        new Student { Id = 3,  Name = "Дмитро",   Surname = "Сидоренко",  Group = "КІ-22" },
        new Student { Id = 4,  Name = "Анна",     Surname = "Коваленко",  Group = "КІ-22" },
        new Student { Id = 5,  Name = "Ігор",     Surname = "Мельник",    Group = "КІ-23" },
        new Student { Id = 6,  Name = "Олена",    Surname = "Бондаренко", Group = "КІ-23" },
        new Student { Id = 7,  Name = "Тарас",    Surname = "Кравченко",  Group = "КІ-21" },
        new Student { Id = 8,  Name = "Вікторія", Surname = "Шевченко",   Group = "КІ-22" },
        new Student { Id = 9,  Name = "Назар",    Surname = "Гриценко",   Group = "КІ-23" },
        new Student { Id = 10, Name = "Юлія",     Surname = "Морозенко",  Group = "КІ-21" },
    };

    static int nextId = 11;

    static readonly JsonSerializerOptions jsonOpts = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    static void Main()
    {
        var listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        listener.Start();
        Console.WriteLine("сервер запущено на http://localhost:8080/");
        Console.WriteLine("Натисніть Ctrl+C для зупинки.");

        while (true)
        {
            var context = listener.GetContext();
            HandleRequest(context);
        }
    }

    static void HandleRequest(HttpListenerContext ctx)
    {
        var req  = ctx.Request;
        var resp = ctx.Response;
        var path = req.Url?.AbsolutePath.TrimEnd('/') ?? "";
        var method = req.HttpMethod.ToUpper();

        Console.WriteLine($"[{method}] {path}");

        try
        {
          
            if (method == "POST" && path == "/student")
            {
                var body = ReadBody(req);
                var newStudent = JsonSerializer.Deserialize<Student>(body, jsonOpts);

                if (newStudent == null)
                {
                    SendResponse(resp, 400, "{ \"error\": \"Невірний формат тіла запиту\" }");
                    return;
                }

                newStudent.Id = nextId++;
                students.Add(newStudent);

                Console.WriteLine($"  Додано студента: {newStudent.Name} {newStudent.Surname} (Id={newStudent.Id})");
                SendResponse(resp, 201, JsonSerializer.Serialize(newStudent, jsonOpts));
                return;
            }

           
            if (method == "PUT" && path.StartsWith("/student/"))
            {
                var idStr = path.Substring("/student/".Length);

                if (!int.TryParse(idStr, out int id))
                {
                    SendResponse(resp, 400, "{ \"error\": \"Невірний формат Id\" }");
                    return;
                }

                var existing = students.FirstOrDefault(s => s.Id == id);
                if (existing == null)
                {
                    SendResponse(resp, 404, $"{{ \"error\": \"Студент з Id={id} не знайдений\" }}");
                    return;
                }

                var body = ReadBody(req);
                var updated = JsonSerializer.Deserialize<Student>(body, jsonOpts);

                if (updated == null)
                {
                    SendResponse(resp, 400, "{ \"error\": \"Невірний формат тіла запиту\" }");
                    return;
                }

                existing.Name    = updated.Name;
                existing.Surname = updated.Surname;
                existing.Group   = updated.Group;

                Console.WriteLine($"  → Оновлено студента Id={id}: {existing.Name} {existing.Surname}");
                SendResponse(resp, 200, JsonSerializer.Serialize(existing, jsonOpts));
                return;
            }

           
            if (method == "GET" && path == "/student")
            {
                SendResponse(resp, 200, JsonSerializer.Serialize(students, jsonOpts));
                return;
            }

           
            SendResponse(resp, 404, "{ \"error\": \"Маршрут не знайдений\" }");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"  ! Помилка: {ex.Message}");
            SendResponse(resp, 500, $"{{ \"error\": \"{ex.Message}\" }}");
        }
    }

    static string ReadBody(HttpListenerRequest req)
    {
        using var reader = new System.IO.StreamReader(req.InputStream, req.ContentEncoding);
        return reader.ReadToEnd();
    }

    static void SendResponse(HttpListenerResponse resp, int statusCode, string json)
    {
        resp.StatusCode = statusCode;
        resp.ContentType = "application/json; charset=utf-8";
        var bytes = Encoding.UTF8.GetBytes(json);
        resp.ContentLength64 = bytes.Length;
        resp.OutputStream.Write(bytes, 0, bytes.Length);
        resp.OutputStream.Close();
    }
}
