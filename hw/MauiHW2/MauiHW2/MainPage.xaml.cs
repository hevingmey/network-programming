
using System.Net.Sockets;
using System.Text;
namespace MauiHW2;



public partial class MainPage : ContentPage
{
    private TcpClient? client;
    private NetworkStream? stream;

    public MainPage()
    {
        InitializeComponent();
    }

    private async void ConnectButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            string address = ServerAddressEntry.Text;
            int port = int.Parse(PortEntry.Text);

            client = new TcpClient();
            await client.ConnectAsync(address, port);

            stream = client.GetStream();

            string response = await ReadMessageAsync();

            UpdateScreen(response);

            HistoryLabel.Text += "Connected to server\n\n";
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void SendLetterButton_Clicked(object sender, EventArgs e)
    {
        if (client == null || stream == null)
        {
            await DisplayAlert("Error", "Connect to server first", "OK");
            return;
        }

        string letter = LetterEntry.Text?.Trim().ToLower() ?? "";

        if (string.IsNullOrWhiteSpace(letter))
        {
            await DisplayAlert("Error", "Enter a letter", "OK");
            return;
        }

        byte[] data = Encoding.UTF8.GetBytes(letter);
        await stream.WriteAsync(data, 0, data.Length);

        string response = await ReadMessageAsync();

        UpdateScreen(response);

        LetterEntry.Text = "";
    }

    private async Task<string> ReadMessageAsync()
    {
        byte[] buffer = new byte[1024];

        int bytesRead = await stream!.ReadAsync(buffer, 0, buffer.Length);

        return Encoding.UTF8.GetString(buffer, 0, bytesRead);
    }

    private void UpdateScreen(string response)
    {
        HistoryLabel.Text += response + "\n\n";

        string[] lines = response.Split('\n');

        foreach (string line in lines)
        {
            if (line.StartsWith("Word:"))
            {
                BoardLabel.Text = line.Replace("Word:", "").Trim();
            }
        }
    }
}