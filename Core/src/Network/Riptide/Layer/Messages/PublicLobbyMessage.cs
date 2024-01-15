using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabFusion.Riptide.Utilities;
using Riptide;

namespace LabFusion.Riptide.Messages
{
    public class PublicLobbyMessage
    {
        public static Message CreateRequestLobbiesMessage()
        {
            return Message.Create(MessageSendMode.Reliable, MessageTypes.RequestLobbies);
        }

        public static Message CreatePublicLobbyMessage()
        {
            return Message.Create(MessageSendMode.Reliable, MessageTypes.CreateLobby);
        }

        public static Message CreateUpdateLobbyMessage(string key, string data)
        {
            var message = Message.Create(MessageSendMode.Reliable, MessageTypes.UpdateLobby);
            message.AddString(key);
            message.AddString(data);
            return message;
        }

        public static void HandleDisconnectMessage(Message message)
        {

        }
    }
}
