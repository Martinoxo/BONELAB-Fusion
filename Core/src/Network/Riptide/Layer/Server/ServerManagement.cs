using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Riptide;

namespace LabFusion.Network.Riptide
{
    public static class ServerManagement
    {
        public static Server _currentServer = new();
        private static int ServerPort = 7777;

        public static void StartServer()
        {
            if (_currentServer.IsRunning)
            {
                StopServer();
                return;
            }

            _currentServer.TimeoutTime = 60;
        }

        public static void StopServer()
        {
            if (!_currentServer.IsRunning)
                return;
        }
    }
}
