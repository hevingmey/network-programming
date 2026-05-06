namespace NewhwStudent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

class Program
{
    static StudentRepository repository = new StudentRepository();

    static void Main(string[] args)
    {
        HttpListener server = new HttpListener();
        server.Prefixes.Add("http://localhost:8080/");
        server.Start();

        Console.WriteLine("Сервер запущено: http://localhost:8080");
        Console.WriteLine("Натисніть Ctrl+C щоб зупинити");

        while (true)
        {
            HttpListenerContext context = server.GetContext();
            HandleRequest(context);
        }
    }

    static void HandleRequest(HttpListenerContext context)
    {
        HttpListenerRequest  request  = context.Request;
        HttpListenerResponse response = context.Response;

        string path   = request.Url.AbsolutePath.TrimEnd('/');
        string method = request.HttpMethod;

        Console.WriteLine($"[{method}] {path}");

        // GET /student
        if (path == "/student" && method == "GET")
        {
            string nameFilter  = request.QueryString["Name"];
            string groupFilter = request.QueryString["Group"];

            bool hasFilter = nameFilter != null || groupFilter != null;

            if (hasFilter)
            {
                List<Student> filtered = repository.GetByFilter(nameFilter, groupFilter);
                string html = GetFilteredHtml(filtered, nameFilter, groupFilter);
                SendResponse(response, html, "text/html", 200);
            }
            else
            {
                List<Student> all = repository.GetAll();
                string html = GetAllStudentsHtml(all);
                SendResponse(response, html, "text/html", 200);
            }

            return;
        }

        // GET /student/id
        if (path.StartsWith("/student/") && method == "GET")
        {
            string idText = path.Replace("/student/", "");

            int id;
            bool parsed = int.TryParse(idText, out id);

            if (!parsed)
            {
                SendResponse(response, "<p>Невірний id</p>", "text/html", 400);
                return;
            }

            Student student = repository.GetById(id);

            if (student == null)
            {
                SendResponse(response, $"<p>Студента з id={id} не знайдено</p>", "text/html", 404);
                return;
            }

            string studentHtml = GetStudentHtml(student);
            SendResponse(response, studentHtml, "text/html", 200);
            return;
        }

        // POST /student
        if (path == "/student" && method == "POST")
        {
            StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
            string body = reader.ReadToEnd();

            Student newStudent = JsonSerializer.Deserialize<Student>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Student added = repository.Add(newStudent);

            string json = JsonSerializer.Serialize(added);
            SendResponse(response, json, "application/json", 201);

            Console.WriteLine($"Додано: {added.Name} {added.Surname}");
            return;
        }

        // PUT /student/id
        if (path.StartsWith("/student/") && method == "PUT")
        {
            string idText = path.Replace("/student/", "");

            int id;
            bool parsed = int.TryParse(idText, out id);

            if (!parsed)
            {
                SendResponse(response, "Невірний id", "text/plain", 400);
                return;
            }

            StreamReader reader = new StreamReader(request.InputStream, Encoding.UTF8);
            string body = reader.ReadToEnd();

            Student updatedData = JsonSerializer.Deserialize<Student>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Student updated = repository.Update(id, updatedData);

            if (updated == null)
            {
                SendResponse(response, $"Студента з id={id} не знайдено", "text/plain", 404);
                return;
            }

            string json = JsonSerializer.Serialize(updated);
            SendResponse(response, json, "application/json", 200);

            Console.WriteLine($"Оновлено студента id={id}");
            return;
        }

        // 404
        SendResponse(response, "<p>404 - сторінку не знайдено</p>", "text/html", 404);
    }

    // --------------------------------------------------------
    // HTML методи
    // --------------------------------------------------------

    static string GetAllStudentsHtml(List<Student> list)
    {
        string html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Студенти</title></head><body>";
        html += "<h1>Список студентів</h1>";
        html += "<ul>";

        for (int i = 0; i < list.Count; i++)
        {
            html += $"<li>{list[i].Id}. {list[i].Name} {list[i].Surname} - {list[i].Group}</li>";
        }

        html += "</ul>";
        html += "</body></html>";

        return html;
    }

    static string GetStudentHtml(Student s)
    {
        string html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Студент</title></head><body>";
        html += "<h1>Інформація про студента</h1>";
        html += "<p>";
        html += $"<b>Id:</b> {s.Id}<br>";
        html += $"<b>Ім'я:</b> {s.Name}<br>";
        html += $"<b>Прізвище:</b> {s.Surname}<br>";
        html += $"<b>Група:</b> {s.Group}";
        html += "</p>";
        html += "</body></html>";

        return html;
    }

    static string GetFilteredHtml(List<Student> list, string name, string group)
    {
        string html = "<!DOCTYPE html><html><head><meta charset='UTF-8'><title>Фільтр</title></head><body>";
        html += "<h1>Результат фільтрації</h1>";
        html += $"<p>Ім'я: {name} | Група: {group}</p>";

        if (list.Count == 0)
        {
            html += "<p>Нічого не знайдено</p>";
        }
        else
        {
            html += "<ul>";

            for (int i = 0; i < list.Count; i++)
            {
                html += $"<li>{list[i].Id}. {list[i].Name} {list[i].Surname} - {list[i].Group}</li>";
            }

            html += "</ul>";
        }

        html += "</body></html>";

        return html;
    }

    // --------------------------------------------------------
    // Відправити відповідь
    // --------------------------------------------------------

    static void SendResponse(HttpListenerResponse response, string content, string contentType, int statusCode)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(content);

        response.StatusCode      = statusCode;
        response.ContentType     = contentType + "; charset=utf-8";
        response.ContentLength64 = buffer.Length;

        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }
}
