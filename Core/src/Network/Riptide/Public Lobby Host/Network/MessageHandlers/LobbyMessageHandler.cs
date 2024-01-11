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
            foreach (var lobby in Core.CurrentLobbies)
            {
                var lobbyResponse = Message.Create(MessageSendMode.Reliable, (ushort)MessageTypes.RequestLobbies);

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

        internal static void HandleUpdateLobbyInfo(Message message, Connection client)
        {

        }

        internal static void HandleJoinLobby(Message message, Connection client)
        {

        }

        internal static void HandleCreateLobby(Message message, Connection client)
        {

        }

        internal static void HandleDeleteLobby(Message message, Connection client)
        {

        }

        internal static void HandleUpdateLobby(Message message, Connection client)
        {

        }
    }
}
