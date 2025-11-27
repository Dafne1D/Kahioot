using System.Net.Sockets;

class Client
{
    private const int port = 5000;
    private const string servidorIP = "127.0.0.1";

    static void Main()
    {
        TcpClient client = null;
        try
        {
            client = new TcpClient(servidorIP, port);
            NetworkStream stream = client.GetStream();
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            string missatge = reader.ReadLine();
            Console.WriteLine($"Servidor: {missatge}");
            Console.Write("Escriu el teu nom: ");
            string nom = Console.ReadLine();
            writer.WriteLine(nom);
            Console.WriteLine("Nom enviat al servidor.");
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Fallada de xarxa.");
            Console.WriteLine(ex.Message);
        }
        catch (IOException ex)
        {
            Console.WriteLine("Error en la comunicació amb el servidor.");
            Console.WriteLine(ex.Message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error {ex.GetType().Name}: {ex.Message}");
        }
        finally
        {
            client?.Close();
        }
    }
}