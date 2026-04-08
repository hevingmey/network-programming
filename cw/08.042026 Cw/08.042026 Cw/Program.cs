using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace _08._042026_Cw
{
    class Program
    {
        private const int _Port = 49152;

        static void Main(string[] args)
        {
            Console.WriteLine("Server started...");
            Accept();                  
            Console.WriteLine("server devo");
            Console.ReadLine();

          
        }

        private static async void Accept()
        {
            await Task.Run(() =>
            {
                try
                {
                    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, _Port);
                    Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    sListener.Bind(ipEndPoint);
                    sListener.Listen(10);

                    

                    while (true)
                    {
                        Socket handler = sListener.Accept();

                        Receive(handler);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine( e.Message);
                }
            });
        }

        private static async void Receive(Socket handler)
        {
            await Task.Run(() =>
            {
                try
                {
                    byte[] bytes = new byte[1024];
                    string data;

                    int bytesRec = handler.Receive(bytes);

                    data = Encoding.Default.GetString(bytes, 0, bytesRec);
                    Console.WriteLine("sms " + data);

                    string reply = "server work +";
                    byte[] msg = Encoding.Default.GetBytes(reply);

                    handler.Send(msg);

                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();

                }
                catch (Exception e)
                {
                    Console.WriteLine( e.Message);

                    try
                    {
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                    }
                    catch { }
                }
            });
        }
    }
}