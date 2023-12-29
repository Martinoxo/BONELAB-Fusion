using LabFusion.Network;
using LabFusion.Riptide.Utilities;
using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;

namespace LabFusion.Riptide.Messages
{
    public class FusionMessage
    {
        public static Message CreateFusionMessage(LabFusion.Network.FusionMessage message, NetworkChannel channel)
        {
            // Create new Riptide message
            Message fusionMessage = Message.Create(ConvertSendMode(channel), MessageTypes.FusionMessage);
            fusionMessage.Release();
            
            // Add byte[] after converting Fusion message
            fusionMessage.AddBytes(message.ToByteArray());

            return fusionMessage;
        }

        private static MessageSendMode ConvertSendMode(NetworkChannel fusionChannel)
        {
            switch (fusionChannel)
            {
                case NetworkChannel.Unreliable:
                    return MessageSendMode.Unreliable;
                case NetworkChannel.Reliable:
                    return MessageSendMode.Reliable;
                case NetworkChannel.VoiceChat:
                    return MessageSendMode.Unreliable;
                default: 
                    return MessageSendMode.Unreliable;
            }
        }
    }
}
