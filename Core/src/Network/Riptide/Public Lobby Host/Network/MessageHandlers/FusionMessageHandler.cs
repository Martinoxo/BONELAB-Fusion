using LobbyHost.Types;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using LobbyHost.UI;

namespace LobbyHost.MessageHandlers
{
    internal class FusionMessageHandler
    {
        public static Message CreateFusionMessage(byte[] data, MessageSendMode channel, ushort messageId = 0)
        {
            return Message.Create(channel, messageId).AddBytes(data);
        }

        internal static void HandleSendToServer(Message message, Connection client)
        {
            byte[] data = message.GetBytes();
            ushort id = message.GetUShort();

            var msg = CreateFusionMessage(data, message.SendMode);
            msg.AddBool(true);
            Core.Server.Send(msg, id);
        }

        internal static void HandleBroadcast(Message message, Connection connection)
        {
            byte[] data = message.GetBytes();
            var lobby = Lobby.GetHostLobby(connection.Id);

            if (lobby != null)
            {
                var msg = CreateFusionMessage(data, message.SendMode);
                var clients = new List<Connection>(lobby.Clients);
                foreach (var client in clients)
                {
                    Core.Server.Send(msg, client.Id, false);
                }
                msg.Release();
            }
        }

        internal static void HandleSendFromServer(Message message, Connection client)
        {
            byte[] data = message.GetBytes();
            ushort id = message.GetUShort();
            bool isHost = message.GetBool();

            var msg = CreateFusionMessage(data, message.SendMode);
            msg.AddBool(isHost);
            Core.Server.Send(msg, id);
        }
    }
}
