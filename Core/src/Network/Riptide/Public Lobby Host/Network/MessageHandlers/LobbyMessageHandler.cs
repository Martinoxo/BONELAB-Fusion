using LobbyHost.Types;
using LobbyHost.UI;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.MessageHandlers
{
    internal class LobbyMessageHandler
    {
        internal static void HandleRequestLobbies(Message message, Connection client)
        {
            if (Core.CurrentLobbies.Count == 0)
            {
                var response = Message.Create(MessageSendMode.Reliable, (ushort)MessageTypes.RequestLobbies);
                response.AddInt(0);
                Core.Server.Send(response, client.Id);
                return;
            }
            foreach (var lobby in Core.CurrentLobbies)
            {
                var lobbyResponse = Message.Create(MessageSendMode.Reliable, (ushort)MessageTypes.RequestLobbies);

                lobbyResponse.AddInt(Core.CurrentLobbies.Count);

                lobbyResponse.AddUShort(lobby.HostId);
                lobbyResponse.AddInt(lobby.Metadata.Count);

                foreach (var metadata in lobby.Metadata)
                {
                    lobbyResponse.AddString(metadata.Key);
                    lobbyResponse.AddString(metadata.Value);
                }

                Core.Server.Send(lobbyResponse, client.Id);
            }
        }

        internal static void HandleJoinLobby(Message message, Connection client)
        {
            ushort hostId = message.GetUShort();
            var lobby = Lobby.GetHostLobby(hostId);
            if (lobby != null)
            {
                lobby.Clients.Add(client);

                var msg = Message.Create(MessageSendMode.Reliable, MessageTypes.JoinLobby);
                Core.Server.Send(msg, client.Id);
                msg.AddBool(true);
            } else
            {
                TUIManager.RefreshUi("Failed to join lobby.");
            }
        }

        internal static void HandleCreateLobby(Message message, Connection client)
        {
            var lobby = new Lobby(client);
            Core.CurrentLobbies.Add(lobby);

            var response = Message.Create(MessageSendMode.Reliable, MessageTypes.CreateLobby);
            Core.Server.Send(response, client.Id);

            TUIManager.RefreshUi($"Created lobby with Id {client.Id}");
        }

        internal static void HandleUpdateLobby(Message message, Connection client)
        {
            string key = message.GetString();
            string value = message.GetString();

            var lobby = Lobby.GetLobby(client.Id);

            // Remove the pair if it exists
            int index = lobby.Metadata.FindIndex(pair => pair.Key == key);
            if (index >= 0) 
                lobby.Metadata.RemoveAt(index);

            // Add metadata to the lobby
            lobby.Metadata.Add(new KeyValuePair<string, string>(key, value));

            TUIManager.RefreshUi($"Obtained lobby key {key} with value {value}");
        }
    }
}
