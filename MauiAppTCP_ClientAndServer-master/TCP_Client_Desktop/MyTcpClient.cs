using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TCP_Client_Desktop;

internal class MyTcpClient
{
    public static event Action<string>? OnLog;
    private static TcpClient? _client;
    private static NetworkStream? _stream;
    private static StreamReader? _streamReader;

  
    public static async Task RunConnectToServerAsync(string url, int port)
    {
        _client = new TcpClient();
        IPAddress? ip;
        try
        {
            ip = IPAddress.Parse(url);
        }
        catch (Exception)
        {
            ip = Dns.GetHostAddresses(url)
                .FirstOrDefault(a => a.AddressFamily == AddressFamily.InterNetwork);

            if (ip == null)
            {
                OnLog?.Invoke($"IPv4 not found");
                throw;
            }
           
        }
        await _client.ConnectAsync(ip, port);
        _stream = _client.GetStream();
        _streamReader = new StreamReader(_stream, Encoding.UTF8);
        OnLog?.Invoke($"Client has been connected to server {_client.Client.LocalEndPoint}");
    }

    public static async Task HandleClientAsync(string message)
    {
        
        string? response = string.Empty;
        if (_client == null || !_client.Connected || _stream == null || _streamReader == null)
        {
            OnLog?.Invoke("Not connected");
            Disconnect();
            return;
        }
        
        byte[] data = Encoding.UTF8.GetBytes(message + "\n");

        try
        {
            await _stream.WriteAsync(data, 0, data.Length);
            string? answerFromServer = await _streamReader.ReadLineAsync();
            if (answerFromServer == null)
                Disconnect();
            else
            {
                OnLog?.Invoke($"Server Response: {answerFromServer}");
            }
                
        }
        catch (Exception ex)
        {
            OnLog?.Invoke($"Send error: {ex.Message}");
            Disconnect();
        }
    }

    private static void Disconnect()
    {
        _client?.Close();
        _stream?.Close();
        _streamReader?.Close();
  
    }
}
