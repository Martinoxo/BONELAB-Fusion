using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.Types
{
    public class RiptideLobbyInfo
    {
        public string Name;
        public string HostName;
        public ushort HostId;
        public byte MaxPlayers;
        public byte CurrentPlayerCount;
    }
}
