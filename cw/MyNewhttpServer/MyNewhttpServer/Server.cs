using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Text.Json;
using System.Web;
using System.Text.Encodings.Web;

namespace MyNewHttpServer;

class User
{
    public string Login { get; set; } = null!;
    public string Pwd { get; set; } = null!;
    public override string ToString()
    {
        return $"Login: {Login} Password {Pwd}";
    }
}

internal class Server
{
    readonly string _HOST = "http://127.0.0.1:8082/";
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
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse res = ctx.Response;
                if (req.HttpMethod == "GET")
                {
                    Console.WriteLine($"Request: {req.Url} {req.HttpMethod} {req.Url?.AbsolutePath}");
                    string? param = req.Url?.AbsolutePath;
                    Console.WriteLine(param);
                    if (param == null)
                    {
                        param = "/";
                    }
                    if (param == "/")
                    {
                        // base url
                        //query params
                        var queryString = req.QueryString;
                        if (queryString != null)
                        {
                            Console.WriteLine($"QUERY PARAMS : {queryString["login"]} {queryString["pwd"]}");
                        }
                    }
                    string page = GetPageName(param);

                    string path = Path.Combine(AppContext.BaseDirectory, "wwwroot", "pages", page);
                    string html = await File.ReadAllTextAsync(path, req.ContentEncoding);
                    byte[] bytes = Encoding.UTF8.GetBytes(html);
                    res.ContentLength64 = bytes.Length;
                    res.ContentType = "text/html; charset=utf-8";
                    res.StatusCode = 200;
                    using (Stream stream = res.OutputStream)
                    {
                        stream.Write(bytes, 0, bytes.Length);
                    }
                }
                else if (req.HttpMethod == "POST")
                {//зчитування тіла запиту
                    string body = "";
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        body = await reader.ReadToEndAsync();
                        try
                        {
                            #region Json
                            // var user = JsonSerializer.Deserialize<User>(body);
                            // byte[] bytes = Encoding.UTF8.GetBytes(body);
                            // res.ContentLength64 = bytes.Length;
                            // res.ContentType = "application/json; charset=utf-8";
                            // res.StatusCode = 200;
                            // using (Stream stream = res.OutputStream)
                            // {
                            //     stream.Write(bytes, 0, bytes.Length);
                            // }
                            #endregion
                           //  Console.WriteLine($"Post{body}");
                           // var formData= HttpUtility.ParseQueryString(body);
                           // Console.WriteLine($"login: {formData["login"]} Password{formData["pwd"]}");
                           // res.Redirect(_HOST);
                           
                           Console.WriteLine($"Post: {body}");

                           var formData = HttpUtility.ParseQueryString(body);

                           string login = formData["login"] ?? "";
                           string password = formData["pwd"] ?? "";
                           string repeatPassword = formData["repeatPwd"] ?? "";
                           string agree = formData["agree"] ?? "";

                           List<string> errors = new List<string>();

                           if (login.Length <= 5)
                           {
                               errors.Add("Login can not be longer than 5 characters");
                           }

                           if (password != repeatPassword)
                           {
                               errors.Add("password isn't equal to repeat password");
                           }

                           if (agree != "true")
                           {
                               errors.Add("user don't aproved regist");
                           }

                           object result;

                           if (errors.Count == 0)
                           {
                               result = new
                               {
                                   login = login,
                                   message = "user registered",
                               };
                           }
                           else
                           {
                               result = new
                               {
                                   errors = errors
                               };
                           }

                           string json = JsonSerializer.Serialize(result, new JsonSerializerOptions
                           {
                               Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                           });
                           byte[] bytes = Encoding.UTF8.GetBytes(json);

                           res.ContentType = "application/json; charset=utf-8";
                           res.ContentLength64 = bytes.Length;
                           res.StatusCode = 200;

                           await res.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                           
                        }
                        catch (Exception ex)
                        {

                            Console.WriteLine(ex.Message);
                        }
                    }
                    
                } 
                res.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    private string GetPageName(string param) // /contacts
    {
        string result = param switch
        {
            "/contacts" => "contacts.html",
            "/about" => "about.html",
            "/" => "index.html",
            _=>"notfound.html"
        };
        return result;
    }
}
