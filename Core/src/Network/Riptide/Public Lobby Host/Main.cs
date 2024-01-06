using System;
using LobbyHost.Types;
using Riptide;
using Riptide.Utils;

namespace LobbyHost
{
    internal class Core
    {
        internal static Server Server;
        internal static List<Lobby> CurrentLobbies = new();

        static void Main(string[] args)
        {
#if DEBUG
            RiptideLogger.Initialize(Console.WriteLine, true);
#endif

            Server = new Server();
            Server.ClientConnected += Hooking.OnClientConnected;
            Server.ClientDisconnected += Hooking.OnClientDisconnected;
            Server.Start(6666, 30);

            while (true)
            {
                Server.Update();

                Thread.Sleep(8);
            }
        }
    }
}
