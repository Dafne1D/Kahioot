using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.Sockets;

class ServerProgram
{
    private const int QuantitatJugadors = 1;
    private const int Port = 5000;

    private static TcpListener Listener;
    private static TcpClient[] Clients = new TcpClient[QuantitatJugadors];
    private static StreamReader[] Readers = new StreamReader[QuantitatJugadors];
    private static StreamWriter[] Writers = new StreamWriter[QuantitatJugadors];
    private static string[] NomClients = new string[QuantitatJugadors];
    private static int[] Puntuacions = new int[QuantitatJugadors];

    private static readonly (string pregunta, string correcta)[] Preguntes =
    {
        ("Quin tipus de pokèmon és en Charmeleon?\n A) Aigua  B) Terra  C) Foc  D) Fantasma", "C"),
        ("Quina d'aquestes opcions no és un ERP?\n A) SAP  B) Netsuite  C) Odoo  D) GIMP", "D"),
        ("En el joc de cartes Màgic, que NO és un encanteri?\n A) Criatures  B) Terres  C) Instantanis  D) Artefactes", "B"),
        ("Qui és el director de la saga de pelis 'The Lord Of The Rings'?\n A) Tim Barton   B) Peter Jackson  C) Brandon Sanderson  D) Uriens Agustí", "B"),
        ("Quin llenguatge de programació´utilitza Unity?\n A) Python B) Anglès C) Java D) C#", "D")
    };
    static async Task Main(string[] args)
    {
        Listener = new TcpListener(IPAddress.Any, Port);
        Listener.Start();

        await AcceptarClients();
        await FerPartida();

        TancarConnexions();
        Listener.Stop();
    }
    static async Task AcceptarClients()
    {
        for (int i = 0; i < QuantitatJugadors; i++)
        {
            Console.WriteLine($"Esperant jugador {i}...");
            Clients[i] = await Listener.AcceptTcpClientAsync();
            NetworkStream ns = Clients[i].GetStream();
            Readers[i] = new StreamReader(ns);
            Writers[i] = new StreamWriter(ns) { AutoFlush = true };

            Writers[i].WriteLine("Escriu el teu nom:");
            NomClients[i] = await Readers[i].ReadLineAsync();
            Puntuacions[i] = 0;
        }

        foreach (var w in Writers)
            w.WriteLine("Tots els jugadors connectats. Iniciem amb les preguntes!");
    }
    static async Task FerPartida()
    {
        foreach (var (pregunta, correcta) in Preguntes)
        {
            await EnviarATots($"Pregunta:\n{pregunta}");

            for (int i = 0; i < QuantitatJugadors; i++)
            {
                Writers[i].WriteLine("Resposta (A/B/C/D):");
            }

            for (int i = 0; i < QuantitatJugadors; i++)
            {
                string resposta = (await Readers[i].ReadLineAsync())?.Trim().ToUpper();
                Console.WriteLine($"{NomClients[i]} ha respost {resposta}");

                if (resposta == correcta)
                {
                    Puntuacions[i] += 20;
                    Writers[i].WriteLine($"Correcte! +20 punts (Total: {Puntuacions[i]})");
                }
                else
                    Writers[i].WriteLine($"Incorrecte. La resposta era {correcta}");
            }
        }
        await EnviarATots("\nResultats:");
        for (int i = 0; i < QuantitatJugadors; i++)
        {
            await EnviarATots($"{NomClients[i]}: {Puntuacions[i]} punts");
        }

        await EnviarATots("S'ha acabat la partida");
    }
    static async Task EnviarATots(string msg)
    {
        foreach (var w in Writers)
            await w.WriteLineAsync(msg);
    }
    static void TancarConnexions()
    {
        for (int i = 0; i < QuantitatJugadors; i++)
        {
            Writers[i]?.Dispose();
            Readers[i]?.Dispose();
            Clients[i]?.Close();
        }
    }
}