using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Web;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace MyNewHttpServer;

class User
{
    public string Login { get; set; } = "";
    public string Pwd { get; set; } = "";
    public string Email { get; set; } = "";

    public override string ToString()
    {
        return $"Login: {Login}, Password: {Pwd}, Email: {Email}";
    }
}

internal class Server
{
    readonly string _HOST = "http://127.0.0.1:8080/";
    HttpListenerRequest? req;

    public async Task RunServer()
    {
        HttpListener server = new HttpListener();
        server.Prefixes.Add(_HOST);
        server.Start();

        Console.WriteLine($"Server has been started {_HOST}");

        while (true)
        {
            try
            {
                HttpListenerContext ctx = await server.GetContextAsync();
                req = ctx.Request;
                HttpListenerResponse res = ctx.Response;

                if (req.HttpMethod == "GET")
                {
                    Console.WriteLine($"Request: {req.Url} {req.HttpMethod} {req.Url?.AbsolutePath}");

                    string param = req.Url?.AbsolutePath ?? "/";

                    string staticPath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        param.TrimStart('/')
                    );

                    if (File.Exists(staticPath) && !param.EndsWith(".html"))
                    {
                        byte[] fileBytes = await File.ReadAllBytesAsync(staticPath);
                        res.ContentLength64 = fileBytes.Length;
                        res.ContentType = GetMimeType(staticPath);
                        res.StatusCode = 200;

                        using (Stream stream = res.OutputStream)
                        {
                            stream.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }
                    else
                    {
                        if (param == "/")
                        {
                            var queryString = req.QueryString;
                            if (queryString != null && queryString.Count != 0)
                            {
                                Console.WriteLine($"QUERY PARAMS : {queryString["login"]} {queryString["pwd"]}");
                            }
                        }

                        string page = GetPageName(param);

                        string pathToContent = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            "pages",
                            page
                        );

                        string html = await File.ReadAllTextAsync(pathToContent, Encoding.UTF8);
                        html = await InsertContent(html);

                        byte[] bytes = Encoding.UTF8.GetBytes(html);

                        res.ContentLength64 = bytes.Length;
                        res.ContentType = "text/html; charset=utf-8";
                        res.StatusCode = 200;

                        using (Stream stream = res.OutputStream)
                        {
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                }
                else if (req.HttpMethod == "POST")
                {
                    string body = "";

                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        body = await reader.ReadToEndAsync();
                    }

                    var formData = HttpUtility.ParseQueryString(body);

                    User user = new User
                    {
                        Login = formData["login"] ?? "",
                        Pwd = formData["pwd"] ?? "",
                        Email = formData["email"] ?? ""
                    };

                    Console.WriteLine(user);

                    await SendEmailAsync(user.Email, user.Login);

                    string html = $@"
        <html>
        <body>
            <h1>Реєстрація успішна</h1>
            <p>Лист відправлено на {user.Email}</p>
        </body>
        </html>";

                    byte[] buffer = Encoding.UTF8.GetBytes(html);

                    res.ContentLength64 = buffer.Length;
                    res.ContentType = "text/html";
                    res.StatusCode = 200;

                    using Stream output = res.OutputStream;
                    await output.WriteAsync(buffer);
                }

                res.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private async Task<string> InsertContent(string content)
    {
        string basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "pages"
        );

        string pathToLayout = Path.Combine(basePath, "layout.html");

        if (!File.Exists(pathToLayout))
        {
            return content;
        }

        string layout = await File.ReadAllTextAsync(pathToLayout, Encoding.UTF8);
        return layout.Replace("{{content}}", content);
    }

    private string GetPageName(string param)
    {
        string result = param switch
        {
            "/contacts" => "contacts.html",
            "/about" => "about.html",
            "/register" => "register.html",
            "/" => "index.html",
            _ => "notfound.html"
        };

        return result;
    }
    private async Task SendEmailAsync(string toEmail, string login)
    {
        MailAddress from = new MailAddress("YOUR_EMAIL@gmail.com", "My Server");
        MailAddress to = new MailAddress(toEmail);

        MailMessage message = new MailMessage(from, to);

        message.Subject = "Реєстрація успішна";
        message.Body = $"Вітаємо {login}! Реєстрація виконана успішно.";

        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);

        smtp.Credentials = new NetworkCredential(
            "YOUR_EMAIL@gmail.com",
            "YOUR_APP_PASSWORD"
        );

        smtp.EnableSsl = true;

        await smtp.SendMailAsync(message);
    }

    private string GetMimeType(string filePath)
    {
        return Path.GetExtension(filePath).ToLower() switch
        {
            ".css"  => "text/css",
            ".js"   => "application/javascript",
            ".jpg"  => "image/jpeg",
            ".jpeg" => "image/jpeg",
            ".png"  => "image/png",
            ".gif"  => "image/gif",
            ".svg"  => "image/svg+xml",
            ".ico"  => "image/x-icon",
            _       => "application/octet-stream"
        };
    }
}