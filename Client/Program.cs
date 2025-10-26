using System.Net;
using System.Net.Sockets;

namespace Chess.Client.Cli
{
    internal class Program
    {
        static async Task<int> Main()
        {
            try
            {
                Client client = await AppFactory.CreateAsync(IPAddress.Loopback, 5000);
                await client.Run();
                return 0;
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return 1;
            }
        }
    }
}