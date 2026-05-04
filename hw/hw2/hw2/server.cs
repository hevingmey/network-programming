using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

class ClientState
{
    public IPEndPoint EndPoint { get; set; }
    public string Name { get; set; }
    public int SecretNumber { get; set; }
    public int Attempts { get; set; }
}

class Program
{
    static UdpClient server = new UdpClient(5000);
    static Dictionary<string, ClientState> clients = new Dictionary<string, ClientState>();
    static Random random = new Random();

    static void Main()
    {
        Console.WriteLine("Сервер запущено на порту 5000...");

        while (true)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] data = server.Receive(ref remoteEP);
            string message = Encoding.UTF8.GetString(data).Trim();
            string key = remoteEP.ToString();

            Console.WriteLine($"[{key}] -> {message}");

            HandleMessage(key, remoteEP, message);
        }
    }

    static void HandleMessage(string key, IPEndPoint ep, string message)
    {
        if (message.StartsWith("JOIN:"))
        {
            string name = message.Substring(5);

            if (clients.ContainsKey(key))
            {
                Send(ep, "INFO:Вже підключений");
                return;
            }

            clients[key] = new ClientState
            {
                EndPoint = ep,
                Name = name,
                SecretNumber = random.Next(1, 101),
                Attempts = 0
            };

            Send(ep, $"WELCOME:{name}|Я загадав число 1-100");
            return;
        }

        if (message == "LIST")
        {
            if (clients.Count == 0)
            {
                Send(ep, "LIST:Немає гравців");
                return;
            }

            string list = string.Join(", ", clients.Values.Select(c => c.Name));
            Send(ep, $"LIST:{list}");
            return;
        }

        if (!clients.ContainsKey(key))
        {
            Send(ep, "ERROR:Спочатку JOIN");
            return;
        }

        var client = clients[key];

        if (message.StartsWith("GUESS:"))
        {
            if (!int.TryParse(message.Substring(6), out int guess))
            {
                Send(ep, "ERROR:Не число");
                return;
            }

            client.Attempts++;

            if (client.Attempts > 10)
            {
                Send(ep, $"LOSE:Програв. Було {client.SecretNumber}");

                client.SecretNumber = random.Next(1, 101);
                client.Attempts = 0;
                return;
            }

            if (guess < client.SecretNumber)
                Send(ep, $"HINT:БІЛЬШЕ ({client.Attempts}/10)");
            else if (guess > client.SecretNumber)
                Send(ep, $"HINT:МЕНШЕ ({client.Attempts}/10)");
            else
            {
                Send(ep, $"WIN:Вгадав за {client.Attempts}");

                client.SecretNumber = random.Next(1, 101);
                client.Attempts = 0;
            }

            return;
        }

        if (message == "EXIT" || message == "QUIT")
        {
            clients.Remove(key);
            Send(ep, "BYE:Вийшов");
            return;
        }

        Send(ep, "ERROR:Команда невідома");
    }

    static void Send(IPEndPoint ep, string msg)
    {
        byte[] data = Encoding.UTF8.GetBytes(msg);
        server.Send(data, data.Length, ep);
    }
}