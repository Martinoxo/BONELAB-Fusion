using LobbyHost.Types;
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
            var lobbyResponse = Message.Create(MessageSendMode.Reliable, (ushort)MessageTypes.RequestLobbies);

            lobbyResponse.AddInt(Core.CurrentLobbies.Count);

            foreach (var lobby in Core.CurrentLobbies)
            {
                lobbyResponse.AddUShort(lobby.HostId);
                lobbyResponse.AddInt(lobby.Metadata.Count);

                foreach (var metadata in lobby.Metadata)
                {
                    lobbyResponse.AddString(metadata.Key);
                    lobbyResponse.AddString(metadata.Value);
                }
            }

            Core.Server.Send(lobbyResponse, client.Id);
        }

        internal static void HandleUpdateLobbyInfo(Message message, Connection client)
        {

        }

        internal static void HandleJoinLobby(Message message, Connection client)
        {

        }

        internal static void HandleCreateLobby(Message message, Connection client)
        {
            var lobby = new Lobby(client);
            Core.CurrentLobbies.Add(lobby);

            var response = Message.Create(MessageSendMode.Reliable, MessageTypes.CreateLobby);
            Core.Server.Send(response, client.Id);
        }

        internal static void HandleDeleteLobby(Message message, Connection client)
        {

        }

        internal static void HandleUpdateLobby(Message message, Connection client)
        {
            string key = message.GetString();
            string value = message.GetString();

            KeyValuePair<string, string> keyValuePair = new KeyValuePair<string, string>(key, value);

            var lobby = Lobby.GetLobby(client.Id);

            foreach (var pair in lobby.Metadata)
            {
                if (pair.Key == key)
                {
                    lobby.Metadata.Remove(pair);
                    lobby.Metadata.Add(keyValuePair);

                    return;
                }
            }

            lobby.Metadata.Add(keyValuePair);
        }
    }
}
