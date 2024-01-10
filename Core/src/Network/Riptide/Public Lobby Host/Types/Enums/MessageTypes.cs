﻿using System;
using System.Collections.Generic; 
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.Types
{
    public class MessageTypes
    {
        public const ushort FusionMessage = 0;

        public const ushort PublicSendToServer = 1;
        public const ushort PublicSendFromServer = 2;
        public const ushort PublicBroadcast = 3;

        public const ushort DeleteLobby = 10;
        public const ushort CreateLobby = 11;
        public const ushort UpdateLobby = 12;

        public const ushort RequestLobbyInfo = 100;
    }
}
