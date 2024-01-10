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
    }
}
