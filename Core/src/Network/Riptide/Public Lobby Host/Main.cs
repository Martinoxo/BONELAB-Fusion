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
        internal static Server Server = new();
        internal static List<Lobby> CurrentLobbies = new();

        static void Main(string[] args)
        {
            Console.Title = "BONELAB Lobby Host";

            Server.ClientConnected += Hooking.OnClientConnected;
            Server.ClientDisconnected += Hooking.OnClientDisconnected;
            Server.MessageReceived += Hooking.OnMessageReceived;
            Server.Start(6666, 30, 0, false);

            System.Timers.Timer riptideTimer = new(5);
            riptideTimer.Elapsed += RiptideTick;
            riptideTimer.AutoReset = true;
            riptideTimer.Enabled = true;

#if DEBUG
            RiptideLogger.Initialize(TUIManager.RefreshUi, true);
#endif

            TUIManager.RefreshUi();
            TUIManager.RefreshCommandUi();
        }

        private static void RiptideTick(object? sender, ElapsedEventArgs e)
        {
            try
            {
                Server.Update();
            }
            catch (Exception ex)
            {
                TUIManager.RefreshUi($"Failed to update Server with exception: {ex.Message}");
            }
        }
    }
}
