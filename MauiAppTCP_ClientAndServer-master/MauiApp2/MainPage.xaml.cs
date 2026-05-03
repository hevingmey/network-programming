namespace MauiApp2;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();

        MyTcpServer.OnServerStarted += port =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                PortLabel.Text = $"Server started on port: {port}";
            });
        };

        MyTcpServer.OnLog += message =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LogEditor.Text += message + "\n";
            });
        };
    }

    private void OnStartClicked(object sender, EventArgs e)
    {
        StartBtn.IsEnabled = false;

        _ = Task.Run(MyTcpServer.RunAsync);
    }
}
