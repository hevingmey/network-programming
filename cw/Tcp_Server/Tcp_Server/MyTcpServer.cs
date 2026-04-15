using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Tcp_Server;

internal class MyTcpServer
{
    private readonly static int _Port = 8080;
    public static event Action<string>? OnLog;
    

    private static async Task HandleClientAsync(TcpClient client)
    {
        NetworkStream stream = client.GetStream();
        
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytesRead = await stream.ReadAsync(buffer);
                if (bytesRead == 0)
                {
                    break;
                }
                string msg =Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"msg: {msg} ");
                Console.WriteLine($"                              from: [{client.Client.RemoteEndPoint}] ");
                string response = $"ok, your message: {msg}";
                await stream.WriteAsync(Encoding.UTF8.GetBytes(response));

            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            
        }
        finally{
            Console.WriteLine($"disconnected from {client.Client.RemoteEndPoint}");
            client.Close();
        }       
    }

    public static async Task RunTcpServerAsync()
    {
        TcpListener listener=new TcpListener(IPAddress.Any, _Port);
        listener.Start();
        Console.WriteLine($"Listening on port {_Port}");
        
        
        while (true)
        {
            TcpClient client= await listener.AcceptTcpClientAsync();
            Console.WriteLine($"client: {client.Client.RemoteEndPoint}");
           _ = HandleClientAsync(client);
        }
        
    }
}