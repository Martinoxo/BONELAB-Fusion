using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobbyHost.Types;

namespace LobbyHost
{
    internal class Hooking
    {
        internal static void OnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
        {
            var args = Lobby.GetDisconnectArgs(e.Client);

            if (args.IsHost)
            {

            }
        }

        internal static void OnClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            
        }
    }
}
