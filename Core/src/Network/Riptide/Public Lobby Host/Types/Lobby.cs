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
        public List<KeyValuePair<string, string>> Metadata = new();
        public List<Connection> Clients = new();
        public ushort HostId;

        public Lobby(Connection hostConnetcion)
        {
            HostId = hostConnetcion.Id;
        }

        internal static Lobby GetLobby(ushort hostId)
        {
            foreach (var lobby in Core.CurrentLobbies)
            {
                if (lobby.HostId == hostId)
                {
                    return lobby;
                }
            }

            return null;
        }

        internal static LobbyDisconnectArgs GetDisconnectArgs(Connection connection)
        {
            LobbyDisconnectArgs args;
            foreach (Lobby lobby in Core.CurrentLobbies)
            {
                if (lobby.HostId == connection.Id)
                {
                    args = new LobbyDisconnectArgs(true, lobby, connection);
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
