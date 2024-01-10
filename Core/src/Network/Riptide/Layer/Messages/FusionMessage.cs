using LabFusion.Network;
using LabFusion.Riptide.Utilities;
using LabFusion.Utilities;
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
        /// <summary>
        /// Creates a Riptide message from a Fusion message.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static Message CreateFusionMessage(LabFusion.Network.FusionMessage message, NetworkChannel channel)
        {
            return Message.Create(ConvertSendMode(channel), MessageTypes.FusionMessage).AddBytes(message.ToByteArray());
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
