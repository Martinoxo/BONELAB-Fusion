using LobbyHost.UI;
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
            if (hostConnetcion != null)
            {
                HostId = hostConnetcion.Id;
                Clients.Add(hostConnetcion);
            }
        }

        internal void OnDisconnect(Connection connection)
        {
            if (connection.Id == HostId)
            {
                var msg = Message.Create(MessageSendMode.Reliable, MessageTypes.LobbyPlayerDisconnect);
                msg.AddBool(true);

                foreach (var client in Clients)
                {
                    Core.Server.Send(msg, client.Id, false);
                }
                msg.Release();

                Core.CurrentLobbies.Remove(this);

                TUIManager.RefreshUi($"Lobby with Host ID {HostId} closed.");
            } else
            {
                var msg = Message.Create(MessageSendMode.Reliable, MessageTypes.LobbyPlayerDisconnect);
                msg.AddBool(false);
                msg.AddUShort(connection.Id);

                Core.Server.Send(msg, HostId);

                Clients.Remove(connection);
                TUIManager.RefreshUi($"Client with ID {connection.Id} disconnected from lobby with Host ID {HostId}.");
            }
        }

        internal static Lobby? GetHostLobby(ushort hostId)
        {
            var lobby = Core.CurrentLobbies.FirstOrDefault(lobby => lobby.HostId == hostId);

            return lobby;
        }

        internal static Lobby? GetLobby(ushort clientId)
        {
            var lobby = Core.CurrentLobbies.FirstOrDefault(lobby => lobby.Clients.Any(client => client.Id == clientId));

            return lobby;
        }
    }
}
