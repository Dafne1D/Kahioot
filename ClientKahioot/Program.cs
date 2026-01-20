using System;
using System.IO;
using System.Net.Sockets;

class Client
{
    private const int port = 5000;
    private static string servidorIP = "127.0.0.1";
    private static bool running = true;


    static void Main(string[] args)
    {

        if (args.Length != 0)
        {
            servidorIP = args[0];
        }

        TcpClient client = new TcpClient(servidorIP, port);
        NetworkStream stream = client.GetStream();
        StreamReader reader = new StreamReader(stream);
        StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

        Console.WriteLine("Connectat al servidor!");

        ConsoleKeyInfo cki;

        Console.Clear();

        Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

        while (running)
        {
            string msg = reader.ReadLine();
            if (msg == null)
            {
                running = false;
            }
            else
            {
                Console.WriteLine($"Servidor: {msg}");

                if (msg.Contains("Escriu el teu nom") ||
                    msg.Contains("Resposta"))
                {
                    Console.Write("> ");
                    string resposta = Console.ReadLine();
                    writer.WriteLine(resposta);
                }
            }
        }
        Console.WriteLine("Connexió tencada pel servidor");
    }

    protected static void myHandler(object sender, ConsoleCancelEventArgs args)
    {
        Console.WriteLine("El client ha pres Crtl+c, tencant connexió");
        args.Cancel = true;
        running = false;
    }
}