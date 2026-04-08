using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ClientTcp
{
    class Program
    {
        private const int _Port = 49152;
        private const string _IP = "127.0.0.1";

        static void Main(string[] args)
        {
            try
            {
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(_IP), _Port);
                Socket sender = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                sender.Connect(ipEndPoint);
                
                Console.WriteLine("connect");

                Console.WriteLine("enter message");                
                string sms = Console.ReadLine();

                byte[] msg = Encoding.Default.GetBytes(sms);
                
                sender.Send(msg);

                byte[] bytes = new byte[1024];
                
                int bytesRec = sender.Receive(bytes);

                string answer = Encoding.Default.GetString(bytes, 0, bytesRec);
                Console.WriteLine("from server: " + answer);

                sender.Shutdown(SocketShutdown.Both);
                sender.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine( e.Message);
            }

            Console.ReadLine();
        }
    }
}