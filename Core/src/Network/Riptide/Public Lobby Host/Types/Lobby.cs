using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.Types
{
    public class Lobby
    {
        public string Name;
        public string HostName;
        public ushort HostId;
        public byte MaxPlayers;
        public byte CurrentPlayerCount;
        public List<Connection> Clients = new();

        public Lobby(Connection hostConnetcion, string name, string hostName, byte maxPlayers, byte currentPlayers)
        {
            Name = name;
            HostName = hostName;
            MaxPlayers = maxPlayers;
            HostId = hostConnetcion.Id;
        }

        internal static LobbyDisconnectArgs GetDisconnectArgs(Connection connection)
        {
            LobbyDisconnectArgs args;
            foreach (Lobby lobby in Core.CurrentLobbies)
            {
                if (lobby.HostId == connection.Id)
                {
                    Core.Server.TryGetClient(lobby.HostId, out Connection hostConnection);
                    args = new LobbyDisconnectArgs(true, lobby, hostConnection);
                    return args;
                }

                foreach (Connection client in lobby.Clients)
                {
                    if (client.Id == connection.Id)
                    {
                        args = new LobbyDisconnectArgs(false, lobby, client);
                        return args;
                    }
                }
            }

            return null;
        }
    }
}
