using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace First_Lessone;

public class PortScaner
{
     private const int _PORT = 49152;
     private const string _IP="127.0.0.1";//local host

     public static void FastScaner()
     {
          IPAddress ipAddress = IPAddress.Parse(_IP);

      List<Task> _tasks = new List<Task>();
      List<int> portOpen=new List<int>();
      object locker = new object();

          for (int i = 100; i <= 10000; i++)
          {
               int w = i;
           _tasks.Add(Task.Run(() =>
           {

                try
                {
                     IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, w);
                     Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                     socket.Connect(ipEndPoint);
                     

                     Console.WriteLine($"Port {w} is open");
                     lock (locker)
                     {
                          portOpen.Add(w);
                     }

                }
                catch 
                {
                     
                }
                
           }));    
          }
          Task.WaitAll(_tasks.ToArray());
          string file = "port- " + DateTime.Now.ToString("yyyy-MMdd_HHmmss") + ".txt";
          using (StreamWriter sw = new StreamWriter(file)){
          foreach (int item in portOpen)
          {
               sw.WriteLine(item);
          }
          
          }

          Console.WriteLine("file saved");
     }
     

     public static void Scan()
     {          //Ipv4 x.x.x.x

          //IP adres IP - internet Protocol
          IPAddress ipAddress = IPAddress.Parse(_IP);
          for (int i = 20; i < 7050; i++)
          {
               Console.WriteLine("cheking port: " + i);
               try
               {
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, i);
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    
                    socket.Connect(ipEndPoint);
                    Console.WriteLine($"Port {i} is listening");
               }
               catch (SocketException se)
               {
                    if (se.ErrorCode == 10061)
                    {
                         Console.WriteLine(se.Message);
                    }
               }
          }
     }
     public static void SearchPort()
     {
          Console.WriteLine("Please enter the port you want to scan: ");
          int port = int.Parse(Console.ReadLine());
         IPAddress ipAddress = IPAddress.Parse(_IP);
         try
         {
               IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
               Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
               socket.Connect(ipEndPoint);
               Console.WriteLine($"Port {port} is open");
         }
         catch (SocketException se)
         {
              Console.WriteLine(se);
         }
     }

     
}