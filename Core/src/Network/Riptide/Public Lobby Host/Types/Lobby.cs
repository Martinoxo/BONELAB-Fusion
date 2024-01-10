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
        public RiptideLobbyInfo LobbyInfo = new();
        public List<Connection> Clients = new();

        public Lobby(Connection hostConnetcion, string name, string hostName, byte maxPlayers, byte currentPlayers)
        {
            LobbyInfo.HostId = hostConnetcion.Id;
            LobbyInfo.Name = name;
            LobbyInfo.HostName = hostName;
            LobbyInfo.MaxPlayers = maxPlayers;
            LobbyInfo.CurrentPlayerCount = currentPlayers;
        }

        internal static LobbyDisconnectArgs GetDisconnectArgs(Connection connection)
        {
            LobbyDisconnectArgs args;
            foreach (Lobby lobby in Core.CurrentLobbies)
            {
                if (lobby.LobbyInfo.HostId == connection.Id)
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
