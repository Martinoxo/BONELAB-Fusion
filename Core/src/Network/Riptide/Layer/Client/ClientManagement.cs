using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LabFusion.Representation;
using LabFusion.Riptide.Utilities;
using LabFusion.Senders;
using LabFusion.Utilities;
using Riptide;

namespace LabFusion.Riptide
{
    public class ClientManagement
    {
        public static Client CurrentClient = new();

        public static void P2PJoinServer(string code, ushort port)
        {
            if (CurrentClient.IsConnecting)
                return;

            if (string.IsNullOrEmpty(code))
            {
                FusionNotification serverCodeWarning = new FusionNotification()
                {
                    title = "No Server Code",
                    showTitleOnPopup = true,
                    message = $"You have not entered a server code to join! Please click on the \"Server Code\" button to enter a server code!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.WARNING
                };
                FusionNotifier.Send(serverCodeWarning);

                return;
            }

            if (ServerManagement.CurrentServer.IsRunning)
                ServerManagement.CurrentServer.Stop();

            if (CurrentClient.IsConnected)
                CurrentClient.Disconnect();

            if (!code.Contains("."))
                code = IPExtensions.DecodeIpAddress(code);

            CurrentClient.Connected += OnConnectToP2PServer;

            CurrentClient.Connect($"{code}:{port}");
        }

        private static void OnConnectToP2PServer(object sender, EventArgs e)
        {
            CurrentClient.Connected -= OnConnectToP2PServer;

            PlayerIdManager.SetLongId(CurrentClient.Id);

            ConnectionSender.SendConnectionRequest();
        }
    }
}
