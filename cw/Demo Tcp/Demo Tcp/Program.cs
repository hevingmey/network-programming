using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Demo_Tcp
{
    public partial class Form1 : Form
    {
        private const int _PORT = 49152;
        public SynchronizationContext uiContext;
        public Form1()
        {
            InitializeComponent();
            // Отримаємо контекст синхронізації для поточного потоку
            uiContext = SynchronizationContext.Current;
        }

        /*
        SOCKET – це логічне "гніздо", яке дозволяє двом програмам обмінюватися інформацією
        по мережі, не замислюючись про місцезнаходження.
        SOCKET – це комбінація IP-адреси та номера порту.

        Internet Protocol (IP) – широко використовуваний протокол у локальних і глобальних мережах.
        Цей протокол не гарантує доставку даних, тому для надійної передачі використовують TCP або UDP.

        Transmission Control Protocol (TCP) встановлює з’єднання та забезпечує
        безпомилкову передачу даних між комп’ютерами.

        User Datagram Protocol (UDP) працює без встановлення з’єднання,
        не гарантує надійності, але дозволяє передавати дані багатьом адресатам одночасно.
        TCP та UDP працюють поверх IP, тому кажуть TCP/IP або UDP/IP.
        */

        // Обслуговування чергового запиту будемо виконувати в окремому потоці
        private async void Receive(Socket handler)
        {
            await Task.Run(() =>
            {
                try
                {
                    string client = null;
                    string data = null;
                    byte[] bytes = new byte[1024];

                    // Отримаємо від клієнта DNS-ім’я хоста.
                    // Метод Receive отримує дані з сокета та заповнює масив байтів
                    int bytesRec = handler.Receive(bytes); // Повертає фактичну кількість отриманих байтів
                    client = Encoding.Default.GetString(bytes, 0, bytesRec); // Конвертуємо масив байтів у рядок
                    client += "(" + handler.RemoteEndPoint.ToString() + ")";

                    while (true)
                    {
                        bytesRec = handler.Receive(bytes); // Отримуємо дані від клієнта. Потік блокується, якщо даних немає
                        if (bytesRec == 0)
                        {
                            handler.Shutdown(SocketShutdown.Both); // Блокуємо передачу та отримання даних
                            handler.Close(); // Закриваємо сокет
                            return;
                        }

                        data = Encoding.Default.GetString(bytes, 0, bytesRec); // Конвертуємо масив байтів у рядок

                        // uiContext.Send надсилає синхронне повідомлення у контекст синхронізації
                        uiContext.Send(d => listBox1.Items.Add(client), null); // Додаємо у список ім’я клієнта
                        uiContext.Send(d => listBox1.Items.Add(data), null); // Додаємо у список повідомлення від клієнта

                        if (data.IndexOf("<end>") > -1) // Якщо клієнт надіслав команду <end>, закінчуємо обробку
                            break;
                    }

                    string theReply = "Я завершу обробку повідомлень";
                    byte[] msg = Encoding.Default.GetBytes(theReply); // Конвертуємо рядок у масив байтів
                    handler.Send(msg); // Відправляємо повідомлення клієнту
                    handler.Shutdown(SocketShutdown.Both); // Блокуємо передачу та отримання
                    handler.Close(); // Закриваємо сокет
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Сервер: " + ex.Message);
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                }
            });
        }

        // Очікування запитів на з’єднання будемо виконувати в окремому потоці
        private async void Accept()
        {
            await Task.Run(() =>
            {
                try
                {
                    // Встановимо для сокета адресу локальної кінцевої точки
                    // Унікальна адреса для TCP/IP = IP-адреса хоста + номер порту
                    IPEndPoint ipEndPoint = new IPEndPoint(
                        IPAddress.Any, // Сервер слухає на всіх мережевих інтерфейсах
                        _PORT // порт
                    );

                    // Створюємо потоковий сокет
                    Socket sListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    /*
                      AddressFamily.InterNetwork – використання IPv4
                      SocketType.Stream – надійний двосторонній потік
                      ProtocolType.Tcp – протокол TCP
                    */

                    // Прив’язуємо сокет до локальної кінцевої точки
                    sListener.Bind(ipEndPoint);

                    // Встановлюємо сокет у стан прослуховування
                    sListener.Listen(10); // Максимальна черга очікуючих підключень

                    while (true)
                    {
                        // Метод Accept блокує потік до надходження запиту на з’єднання
                        // Після прийому створюється новий сокет для обслуговування клієнта
                        Socket handler = sListener.Accept();
                        // Обслуговування клієнта у окремій асинхронній задачі
                        Receive(handler);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Сервер: " + ex.Message);
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Accept(); // Запуск прийому підключень після натискання кнопки
        }
    }
}




    using System;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace TCP_client
{
    public partial class Form1 : Form
    {
        Socket sock;
        private const int _PORT = 49152;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Connect()
        {
            await Task.Run(() =>
            {
                // Підключаємося до віддаленого пристрою
                try
                {
                    IPAddress ipAddr = IPAddress.Parse(ip_address.Text);
                    //string host = ip_address.Text.Split(':')[0]; // витягуємо хост без порту
                    //int port = int.Parse(ip_address.Text.Split(':')[1]); // витягуємо порт

                    //IPAddress ipAddr = Dns.GetHostAddresses(host)[0]; // перетворюємо доменне ім’я у IP

                    // Встановлюємо віддалену кінцеву точку для сокета
                    // Унікальна адреса для TCP/IP = IP-адреса хоста + номер порту
                    IPEndPoint ipEndPoint = new IPEndPoint(ipAddr /* IP-адреса */, _PORT /* порт */);

                    // Створюємо потоковий сокет
                    sock = new Socket(AddressFamily.InterNetwork /* схема адресації */,
                                      SocketType.Stream /* тип сокета */,
                                      ProtocolType.Tcp /* протокол */);
                    /* Значення InterNetwork означає використання IPv4.
                       SocketType.Stream забезпечує надійний двосторонній потік даних з встановленням з’єднання, 
                       без дублювання і без збереження меж повідомлень. 
                       Сокет цього типу взаємодіє з одним вузлом і вимагає попереднього підключення до віддаленого вузла. 
                       Stream використовує протокол TCP і схему адресації AddressFamily.
                    */

                    // Підключаємо сокет до віддаленої кінцевої точки
                    sock.Connect(ipEndPoint);
                    byte[] msg = Encoding.Default.GetBytes(Dns.GetHostName() /* ім’я локального хоста */); // конвертуємо ім’я хоста в масив байтів
                    int bytesSent = sock.Send(msg); // надсилаємо серверу повідомлення через сокет
                    MessageBox.Show("Клієнт " + Dns.GetHostName() + " встановив з’єднання з " + sock.RemoteEndPoint.ToString());
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клієнт: " + ex.Message);
                }
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Connect();
        }

        private async void Exchange()
        {
            await Task.Run(() =>
            {
                try
                {
                    string theMessage = textBox1.Text; // отримуємо текст повідомлення з текстового поля
                    byte[] msg = Encoding.Default.GetBytes(theMessage); // конвертуємо рядок у масив байтів
                    int bytesSent = sock.Send(msg); // надсилаємо повідомлення серверу через сокет
                    if (theMessage.IndexOf("<end>") > -1) // якщо клієнт надіслав команду <end>, приймаємо відповідь від сервера
                    {
                        byte[] bytes = new byte[1024];
                        int bytesRec = sock.Receive(bytes); // приймаємо дані від сервера. Потік блокується, якщо даних немає
                        MessageBox.Show("Сервер (" + sock.RemoteEndPoint.ToString() + ") відповів: " + Encoding.Default.GetString(bytes, 0, bytesRec) /* конвертуємо масив байтів у рядок */);
                        sock.Shutdown(SocketShutdown.Both); // блокуємо передачу та прийом даних для сокета
                        sock.Close(); // закриваємо сокет
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Клієнт: " + ex.Message);
                }
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Exchange();
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                sock.Shutdown(SocketShutdown.Both); // блокуємо передачу та прийом даних для сокета
                sock.Close(); // закриваємо сокет
            }
            catch (Exception ex)
            {
                MessageBox.Show("Клієнт: " + ex.Message);
            }
        }
    }
}