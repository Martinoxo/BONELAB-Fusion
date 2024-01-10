using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.Utilities
{
    public static class ServerInfo
    {
        public static int LobbyCount
        {
            get
            {
                return Core.CurrentLobbies.Count;
            }
        }

        public static int PlayerCount
        {
            get
            {
                return Core.Server.ClientCount;
            }
        }
    }
}
