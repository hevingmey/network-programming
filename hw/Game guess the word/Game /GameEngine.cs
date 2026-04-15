using System.Globalization;
using System.Net.Sockets;
using System.Net;
using System.Text;
namespace Game;

internal class GameEngine
{
    private readonly static int _Port = 5050;

    private static async Task EngineAsync(TcpClient client)
    {
        int random = new Random().Next(1, 50);
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer,0,buffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                string msg = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                Console.WriteLine($"msg: {msg} ");
                Console.WriteLine($"                              from: [{client.Client.RemoteEndPoint}] ");

                if (!int.TryParse(msg, out int number))
                {
                    string error = $"Invalid number: ";
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(error));
                    continue;
                    
                }

                string p = " ";
                string choice;
                if (number < random)
                {
                     choice = "more";
                }
                else if (number > random)
                {
                     choice = "less";
                }
                else
                {
                     choice = "You won!";

                }
                await stream.WriteAsync(Encoding.UTF8.GetBytes(choice));

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);

        }
        finally
        {
            Console.WriteLine($"disconnected from {client.Client.RemoteEndPoint}");
            client.Close();
        }
    }

    public static async Task RunEngineAsync()
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _Port);
        listener.Start();
        Console.WriteLine($"Listening on port {_Port}");
        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine($"client: {client.Client.RemoteEndPoint}");
            _ =EngineAsync(client);
        }
    }
}