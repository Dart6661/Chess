namespace Chess.Server
{
    internal class Program
    {
        static async Task Main()
        {
            App app = new();
            await app.Run();
        }
    }
}
