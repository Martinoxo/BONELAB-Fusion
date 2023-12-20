using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LabFusion.Riptide.Preferences;
using Riptide;

namespace LabFusion.Riptide
{
    public static class ServerManagement
    {
        public static Server CurrentServer = new();

        public static void StartServer()
        {
            if (CurrentServer.IsRunning)
                CurrentServer.Stop();

            CurrentServer.Start(RiptidePreferences.LocalServerSettings.ServerPort.GetValue(), 256);
        }
    }
}
