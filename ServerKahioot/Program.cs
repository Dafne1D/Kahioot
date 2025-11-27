using System.Net;
using System.Net.Sockets;

class ServerProgram
{
    private const int QuantitatJugadors = 2;
    private const int Port = 5000;

    private static TcpListener? Listener;
    private static readonly TcpClient?[] Clients = new TcpClient[QuantitatJugadors];
    private static readonly StreamReader?[] Readers = new StreamReader[QuantitatJugadors];
    private static readonly StreamWriter?[] Writers = new StreamWriter[QuantitatJugadors];
    private static readonly string[] NomClients = new string[QuantitatJugadors];

    static async Task Main()
    {
        try
        {
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();
            Console.WriteLine($"Servidor escoltant al port {Port}...");

            await EsperarTotsElsJugadors();
            await FerPreguntaATots();
        }
        catch (SocketException ex)
        {
            Console.WriteLine($"Fallada de xarxa: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error crític al servidor: {ex.Message}");
        }
        finally
        {
            await TancarTotesLesConnexions();
            Listener?.Stop();
        }
    }

    static async Task EsperarTotsElsJugadors()
    {
        Task[] connexions = new Task[QuantitatJugadors];
        for (int id = 0; id < QuantitatJugadors; id++)
        {
            int idLocal = id;
            connexions[id] = Task.Run(() =>
            {
                GestionarConnexio(idLocal);
                DonarBenvinguda(idLocal);
            });
        }
        await Task.WhenAll(connexions);
        Console.WriteLine($"Tots els {QuantitatJugadors} jugadors connectats!");
    }

    static void GestionarConnexio(int idClient)
    {
        Console.WriteLine($"Esperant la connexió del client {idClient}...");
        Clients[idClient] = Listener.AcceptTcpClient();
        NetworkStream stream = Clients[idClient]!.GetStream();
        Readers[idClient] = new StreamReader(stream);
        Writers[idClient] = new StreamWriter(stream) { AutoFlush = true };
        Console.WriteLine($"Client {idClient} connectat!");
    }

    static void DonarBenvinguda(int idClient)
    {
        Writers[idClient].WriteLine("Benvingut al KAHOOT!");
        Writers[idClient].WriteLine("Quin és el teu nom?");
        string? nom = Readers[idClient].ReadLine();
        NomClients[idClient] = nom;
        Console.WriteLine($"Client {idClient} ha dit que es diu {nom}");
        Writers[idClient].WriteLine("Si us plau, espera a que es connectin la resta dels jugadors...");

    }

    static async Task FerPreguntaATots()
    {
        Task[] preguntes = new Task[QuantitatJugadors];
        for (int id = 0; id < QuantitatJugadors; id++)
        {
            int idLocal = id;
            preguntes[id] = Task.Run(() => FerPregunta(idLocal));
        }
        await Task.WhenAll(preguntes);
    }

    static void FerPregunta(int idClient)
    {
        Console.WriteLine($"Fent pregunta al client {idClient} ({NomClients[idClient]})");
        Writers[idClient].WriteLine($"Atenció {NomClients[idClient]}, primera pregunta: A/B/C/D?");

        string? opcio = Readers[idClient].ReadLine();
        opcio = opcio?.Trim().ToUpperInvariant();

        if (opcio == "A")
        {
            Writers[idClient].WriteLine("Correcte!");
            Console.WriteLine($"Client {idClient} ({NomClients[idClient]}) ha respost correctament");
        }
        else
        {
            Writers[idClient].WriteLine("Incorrecte.");
            Console.WriteLine($"Client {idClient} ({NomClients[idClient]}) ha respost: {opcio ?? "(null)"}");
        }

        Writers[idClient].WriteLine("Gràcies per participar, fins un altre.");
    }


    static async Task TancarTotesLesConnexions()
    {
        Task[] tancaments = new Task[QuantitatJugadors];
        for (int id = 0; id < QuantitatJugadors; id++)
        {
            int idLocal = id;
            tancaments[id] = Task.Run(() => TancarConnexio(idLocal));
        }
        await Task.WhenAll(tancaments);
    }

    static void TancarConnexio(int idClient)
    {
        Console.WriteLine($"Tancant connexió del client {idClient}...");
        Readers[idClient]?.Dispose();
        Writers[idClient]?.Dispose();
        Clients[idClient]?.Close();
        Console.WriteLine($"Connexió del client {idClient} tancada");
    }
}