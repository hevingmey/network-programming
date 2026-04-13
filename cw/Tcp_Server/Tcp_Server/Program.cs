namespace Tcp_Server;

class Program
{
    static async Task Main(string[] args)
    {
        await MyTcpServer.RunTcpServerAsync();
    }
}