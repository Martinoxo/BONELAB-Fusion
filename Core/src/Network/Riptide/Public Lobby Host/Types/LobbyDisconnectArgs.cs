using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.Types
{
    internal class LobbyDisconnectArgs
    {
        public bool IsHost;
        public Lobby Lobby;
        public Connection Client;

        public LobbyDisconnectArgs(bool isHost, Lobby lobby, Connection client)
        {
            IsHost = isHost;
            Lobby = lobby;
            Client = client;
        }
    }
}
