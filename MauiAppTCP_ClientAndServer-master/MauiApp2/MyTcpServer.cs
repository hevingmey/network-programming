using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MauiApp2
{
    internal class MyTcpServer
    {
        private static readonly int _PORT = 5000;

        public static event Action<string>? OnLog;
        public static event Action<int>? OnServerStarted;

        private static async Task HandleClientAsync(TcpClient client)
        {
            try
            {
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);

                while (true)
                {
                    string? messageFromClient = await reader.ReadLineAsync();

                    if (messageFromClient == null)
                        break;

                    OnLog?.Invoke($"Server received: {messageFromClient}");

                    byte[] responseBytes = Encoding.UTF8.GetBytes(
                        $"Your message was: {messageFromClient}\n"
                    );

                    await stream.WriteAsync(responseBytes, 0, responseBytes.Length);
                }
            }
            catch (Exception ex)
            {
                OnLog?.Invoke($"Error: {ex.Message}");
            }
            finally
            {
                OnLog?.Invoke($"Client disconnected: {client.Client.RemoteEndPoint}");
                client.Close();
            }
        }

        public static async Task RunAsync()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _PORT);

            listener.Start();

            OnServerStarted?.Invoke(_PORT);

            OnLog?.Invoke($"Server started on port {_PORT}");

            while (true)
            {
                TcpClient client = await listener.AcceptTcpClientAsync();

                OnLog?.Invoke($"Client connected: {client.Client.RemoteEndPoint}");

                _ = Task.Run(() => HandleClientAsync(client));
            }
        }
    }
}}