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
using LabFusion.Network;
using System.Drawing;
using System.Runtime.Remoting.Messaging;

namespace LabFusion.Riptide
{
    public class ClientManagement
    {
        public static Client CurrentClient = new();

        public static void P2PJoinServer(string code, ushort port)
        {
            if (CurrentClient.IsConnecting)
                return;

            if (CurrentClient.IsPending)
                return;


            if (string.IsNullOrEmpty(code))
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "No Server Code",
                    showTitleOnPopup = true,
                    message = $"You have not entered a server code to join! Please click on the \"Server Code\" button to enter a server code!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.WARNING
                });

                return;
            }

            if (ServerManagement.CurrentServer.IsRunning)
                ServerManagement.CurrentServer.Stop();

            if (CurrentClient.IsConnected)
                CurrentClient.Disconnect();

            if (!code.Contains("."))
                code = IPExtensions.DecodeIpAddress(code);

            CurrentClient.Connected += OnConnectToP2PServer;

            CurrentClient.Connect($"{code}:{port}", 5, 0, null, false);
        }

        private static void OnConnectToP2PServer(object sender, EventArgs e)
        {
            CurrentClient.Connected -= OnConnectToP2PServer;

            CurrentClient.TimeoutTime = 30000;
            CurrentClient.Connection.CanQualityDisconnect = false;
            
            PlayerIdManager.SetLongId(CurrentClient.Id);

            ConnectionSender.SendConnectionRequest();

            InternalLayerHelpers.OnUpdateLobby();
        }

        public static void OnDisconnectFromServer(object sender, DisconnectedEventArgs args)
        {
            InternalServerHelpers.OnDisconnect();
        }

        private static string GetDisconnectReason(DisconnectReason disonnectReason)
        {
            switch (disonnectReason)
            {
                case (DisconnectReason.ConnectionRejected):
                    return "Rejected from connecting";
                case (DisconnectReason.Disconnected):
                    return "Disconnected from server";
                case (DisconnectReason.PoorConnection):
                    return "Poor connection to server";
                case (DisconnectReason.TransportError):
                    return "Transport mismatch";
                case (DisconnectReason.Kicked):
                    return "Kicked from server";
                case (DisconnectReason.TimedOut):
                    return "Timed out from server";
                case (DisconnectReason.ServerStopped):
                    return "Server stopped";
                case (DisconnectReason.NeverConnected):
                    return "Never connected to server";
                default:
                    return "Unkown disconnect reason";
            }
        }
        
        public static void OnMessageReceived(object obj, MessageReceivedEventArgs args)
        {
            FusionMessageHandler.ReadMessage(args.Message.GetBytes());
        }
    }
}
