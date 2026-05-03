using System.Net.Sockets;
using System.Text;

namespace TCP_Client_Desktop;

public partial class MainPage : ContentPage
{
    private TcpClient? _client;
    private StreamReader? _reader;
    private StreamWriter? _writer;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnConnectClicked(object sender, EventArgs e)
    {
        try
        {
            string address = AddressEntry.Text.Trim();

            if (!int.TryParse(PortEntry.Text, out int port))
            {
                ContentEditor.Text = "Wrong port";
                return;
            }

            _client = new TcpClient();
            await _client.ConnectAsync(address, port);

            NetworkStream stream = _client.GetStream();

            _reader = new StreamReader(stream, Encoding.UTF8);
            _writer = new StreamWriter(stream, Encoding.UTF8)
            {
                AutoFlush = true
            };

            ContentEditor.Text = $"Connected to {address}:{port}\n";

            MessageEntry.IsVisible = true;
            SendBtn.IsVisible = true;
        }
        catch (Exception ex)
        {
            ContentEditor.Text = $"Connection error: {ex.Message}";
        }
    }

    private async void OnSendMessageClicked(object sender, EventArgs e)
    {
        try
        {
            if (_client == null || !_client.Connected || _reader == null || _writer == null)
            {
                ContentEditor.Text += "Client not connected\n";
                return;
            }

            string message = MessageEntry.Text;

            if (string.IsNullOrWhiteSpace(message))
            {
                ContentEditor.Text += "Message is empty\n";
                return;
            }

            await _writer.WriteLineAsync(message);

            string? response = await _reader.ReadLineAsync();

            ContentEditor.Text += $"Server: {response}\n";
        }
        catch (Exception ex)
        {
            ContentEditor.Text += $"Error: {ex.Message}\n";
        }
    }
}