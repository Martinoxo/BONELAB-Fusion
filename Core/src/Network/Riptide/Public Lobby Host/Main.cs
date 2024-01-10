using System;
using System.Timers;
using LobbyHost.Types;
using LobbyHost.UI;
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
            Console.Title = "BONELAB Lobby Host";

            Server = new Server();
            Server.ClientConnected += Hooking.OnClientConnected;
            Server.ClientDisconnected += Hooking.OnClientDisconnected;
            Server.MessageReceived += Hooking.OnMessageReceived;
            Server.Start(6666, 30);

            System.Timers.Timer riptideTimer = new(5);
            riptideTimer.Elapsed += RiptideTick;
            riptideTimer.Start();

            TUIManager.RefreshUi();
        }

        private static void RiptideTick(object? sender, ElapsedEventArgs e)
        {
            Server.Update();
        }
    }
}
