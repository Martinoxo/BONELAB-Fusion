using LobbyHost.Types;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;

namespace LobbyHost.MessageHandlers
{
    internal class FusionMessageHandler
    {
        internal static void HandleSendToServer(Message message, Connection client)
        {
            Byte[] data = message.GetBytes();
            ushort hostId = message.GetUShort();
            MessageSendMode channel = message.SendMode;

            Core.Server.Send(CreateFusionMessage(data, channel), hostId);
        }

        internal static void HandleSendFromServer(Message message, Connection client)
        {
            Byte[] data = message.GetBytes();
            ushort userId = message.GetUShort();
            MessageSendMode channel = message.SendMode;

            if (Core.Server.TryGetClient(userId, out var connection))
            {
                Core.Server.Send(CreateFusionMessage(data, channel), connection.Id);
            }
        }

        internal static void HandleBroadcast(Message message, Connection client)
        {
            Byte[] data = message.GetBytes();
            ushort hostId = message.GetUShort();
            MessageSendMode channel = message.SendMode;

            var lobby = Lobby.GetLobby(hostId);
            if (lobby == null)
                return;

            foreach (var connection in lobby.Clients)
            {
                Core.Server.Send(CreateFusionMessage(data, channel), connection.Id);
            }
        }

        private static Message CreateFusionMessage(byte[] data, MessageSendMode channel, ushort messageId = 0)
        {
            return Message.Create(channel, messageId).AddBytes(data);
        }
    }
}
