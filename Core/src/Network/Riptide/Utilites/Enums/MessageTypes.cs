using Steamworks.Data;
using System;
using System.Collections.Generic; 
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Utilities
{
    /// <summary>
    /// Different Riptide message types.
    /// </summary>
    public class MessageTypes
    {
        public const ushort FusionMessage = 0;

        public const ushort PublicSendToServer = 1;
        public const ushort PublicSendFromServer = 2;
        public const ushort PublicBroadcast = 3;

        public const ushort LobbyPlayerDisconnect = 10;
        public const ushort CreateLobby = 11;
        public const ushort UpdateLobby = 12;
        public const ushort JoinLobby = 13;

        public const ushort RequestLobbies = 100;
        public const ushort FinishLobbySending = 101;
    }
}
