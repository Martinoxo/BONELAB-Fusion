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
    }
}
