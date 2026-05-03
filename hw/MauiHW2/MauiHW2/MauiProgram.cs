using System.Net;
using System.Net.Sockets;
using System.Text;

string[] words =
{
    "cat", "dog", "apple", "car", "germany", "ukraine", "london", "computer"
};

Random random = new Random();
string secretWord = words[random.Next(words.Length)];
char[] board = new string('_', secretWord.Length).ToCharArray();

int attempts = 6;
List<char> history = new List<char>();

TcpListener server = new TcpListener(IPAddress.Any, 5000);
server.Start();

Console.WriteLine("TCP Server started on port 5000");
Console.WriteLine($"Secret word: {secretWord}");

while (true)
{
    TcpClient client = server.AcceptTcpClient();
    Console.WriteLine("Client connected");

    NetworkStream stream = client.GetStream();

    SendMessage(stream, GetGameState("Game started!"));

    while (attempts > 0 && new string(board) != secretWord)
    {
        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);

        if (bytesRead == 0)
            break;

        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim().ToLower();

        if (string.IsNullOrWhiteSpace(message))
            continue;

        char letter = message[0];

        if (history.Contains(letter))
        {
            SendMessage(stream, GetGameState($"Letter '{letter}' was already used."));
            continue;
        }

        history.Add(letter);

        if (secretWord.Contains(letter))
        {
            for (int i = 0; i < secretWord.Length; i++)
            {
                if (secretWord[i] == letter)
                {
                    board[i] = letter;
                }
            }

            SendMessage(stream, GetGameState($"Good! Letter '{letter}' is in the word."));
        }
        else
        {
            attempts--;
            SendMessage(stream, GetGameState($"Wrong! Letter '{letter}' is not in the word."));
        }
    }

    if (new string(board) == secretWord)
    {
        SendMessage(stream, GetGameState("You won!"));
    }
    else
    {
        SendMessage(stream, GetGameState($"You lost! The word was: {secretWord}"));
    }

    client.Close();

    secretWord = words[random.Next(words.Length)];
    board = new string('_', secretWord.Length).ToCharArray();
    attempts = 6;
    history.Clear();

    Console.WriteLine($"New secret word: {secretWord}");
}

void SendMessage(NetworkStream stream, string message)
{
    byte[] data = Encoding.UTF8.GetBytes(message);
    stream.Write(data, 0, data.Length);
}

string GetGameState(string message)
{
    return
        $"{message}\n" +
        $"Word: {string.Join(" ", board)}\n" +
        $"Attempts left: {attempts}\n" +
        $"History: {string.Join(", ", history)}";
}