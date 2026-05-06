using System.Net;
using System.Text;
using System.Text.Json;

namespace student;
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
        new Student { Id = 1, Name = "Ivan", Surname = "Petrenko", Group = "P-31" },
        new Student { Id = 2, Name = "Oleg", Surname = "Ivanov", Group = "P-31" },
        new Student { Id = 3, Name = "Anna", Surname = "Shevchenko", Group = "P-32" },
        new Student { Id = 4, Name = "Maria", Surname = "Koval", Group = "P-32" },
        new Student { Id = 5, Name = "Dmytro", Surname = "Bondar", Group = "P-33" },
        new Student { Id = 6, Name = "Nazar", Surname = "Tkachenko", Group = "P-33" },
        new Student { Id = 7, Name = "Sofia", Surname = "Melnyk", Group = "P-31" },
        new Student { Id = 8, Name = "Andrii", Surname = "Kravchenko", Group = "P-32" },
        new Student { Id = 9, Name = "Yulia", Surname = "Moroz", Group = "P-33" },
        new Student { Id = 10, Name = "Maksym", Surname = "Lysenko", Group = "P-31" }
    };

    static async Task Main()
    {
        HttpListener server = new HttpListener();

        server.Prefixes.Add("http://localhost:8080/");
        server.Start();

        Console.WriteLine("Server started on http://localhost:8080/");
        Console.WriteLine("Press Ctrl + C to stop server");

        while (true)
        {
            HttpListenerContext context = await server.GetContextAsync();
            _ = Task.Run(() => HandleRequest(context));
        }
    }

    static async Task HandleRequest(HttpListenerContext context)
    {
        string path = context.Request.Url!.AbsolutePath;
        string method = context.Request.HttpMethod;

        if (path == "/student" && method == "GET")
        {
            GetStudents(context);
        }
        else if (path.StartsWith("/student/") && method == "GET")
        {
            GetStudentById(context);
        }
        else if (path == "/student" && method == "POST")
        {
            await AddStudent(context);
        }
        else if (path.StartsWith("/student/") && method == "PUT")
        {
            await UpdateStudent(context);
        }
        else
        {
            SendHtml(context, "<h1>404 Not Found</h1>", 404);
        }
    }

    static void GetStudents(HttpListenerContext context)
    {
        string? name = context.Request.QueryString["Name"];
        string? group = context.Request.QueryString["Group"];

        IEnumerable<Student> result = students;

        if (!string.IsNullOrWhiteSpace(name))
        {
            result = result.Where(s =>
                s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(group))
        {
            result = result.Where(s =>
                s.Group.Contains(group, StringComparison.OrdinalIgnoreCase));
        }

        StringBuilder html = new StringBuilder();

        html.Append("<h1>Students list</h1>");
        html.Append("<ul>");

        foreach (Student student in result)
        {
            html.Append($"<li>{student.Id}. {student.Name} {student.Surname} - {student.Group}</li>");
        }

        html.Append("</ul>");

        SendHtml(context, html.ToString());
    }

    static void GetStudentById(HttpListenerContext context)
    {
        int id = GetIdFromUrl(context);

        Student? student = students.FirstOrDefault(s => s.Id == id);

        if (student == null)
        {
            SendHtml(context, "<h1>Student not found</h1>", 404);
            return;
        }

        string html = $"""
        <h1>Student info</h1>

        <p>
            Id: {student.Id}<br>
            Name: {student.Name}<br>
            Surname: {student.Surname}<br>
            Group: {student.Group}
        </p>
        """;

        SendHtml(context, html);
    }

    static async Task AddStudent(HttpListenerContext context)
    {
        string body;

        using (StreamReader reader = new StreamReader(context.Request.InputStream))
        {
            body = await reader.ReadToEndAsync();
        }

        Student? newStudent = JsonSerializer.Deserialize<Student>(
            body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (newStudent == null)
        {
            SendHtml(context, "<h1>Invalid student data</h1>", 400);
            return;
        }

        if (string.IsNullOrWhiteSpace(newStudent.Name) ||
            string.IsNullOrWhiteSpace(newStudent.Surname) ||
            string.IsNullOrWhiteSpace(newStudent.Group))
        {
            SendHtml(context, "<h1>Student fields cannot be empty</h1>", 400);
            return;
        }

        if (newStudent.Id == 0)
        {
            newStudent.Id = students.Max(s => s.Id) + 1;
        }

        students.Add(newStudent);

        SendHtml(context, $"""
        <h1>Student added</h1>
        <p>{newStudent.Id}. {newStudent.Name} {newStudent.Surname} - {newStudent.Group}</p>
        """, 201);
    }

    static async Task UpdateStudent(HttpListenerContext context)
    {
        int id = GetIdFromUrl(context);

        Student? student = students.FirstOrDefault(s => s.Id == id);

        if (student == null)
        {
            SendHtml(context, "<h1>Student not found</h1>", 404);
            return;
        }

        string body;

        using (StreamReader reader = new StreamReader(context.Request.InputStream))
        {
            body = await reader.ReadToEndAsync();
        }

        Student? updatedStudent = JsonSerializer.Deserialize<Student>(
            body,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

        if (updatedStudent == null)
        {
            SendHtml(context, "<h1>Invalid student data</h1>", 400);
            return;
        }

        student.Name = updatedStudent.Name;
        student.Surname = updatedStudent.Surname;
        student.Group = updatedStudent.Group;

        SendHtml(context, $"""
        <h1>Student updated</h1>
        <p>{student.Id}. {student.Name} {student.Surname} - {student.Group}</p>
        """);
    }

    static int GetIdFromUrl(HttpListenerContext context)
    {
        string[] parts = context.Request.Url!.AbsolutePath.Split('/');

        if (parts.Length < 3 || !int.TryParse(parts[2], out int id))
        {
            return -1;
        }

        return id;
    }

    static void SendHtml(HttpListenerContext context, string html, int statusCode = 200)
    {
        string page = $"""
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset="UTF-8">
            <title>Student Server</title>
        </head>
        <body>
            {html}
        </body>
        </html>
        """;

        byte[] buffer = Encoding.UTF8.GetBytes(page);

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = buffer.Length;

        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();
    }
}