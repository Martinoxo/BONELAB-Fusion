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
using LabFusion.Riptide.Preferences;

namespace LabFusion.Riptide
{
    public class ClientManagement
    {
        public static Client CurrentClient = new();
        public static Client PublicLobbyClient = new();

        public static bool IsConnecting { get; private set; }

        /// <summary>
        /// Connects to a server, if the player is not already connecting to a server.
        /// </summary>
        /// <param name="code"></param>
        /// <param name="port"></param>
        public static void P2PJoinServer(string code, ushort port)
        {
            if (IsConnecting)
            {
                if (ClientManagement.IsConnecting)
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        showTitleOnPopup = false,
                        message = $"Already connecting to a server!",
                        isMenuItem = false,
                        isPopup = true,
                        popupLength = 5f,
                        type = NotificationType.WARNING
                    });
                }
                return;
            }

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

            if (!uint.TryParse(code, out uint codeInt) && !code.Contains("."))
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    showTitleOnPopup = false,
                    message = $"Invalid server code!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.ERROR
                });
                return;
            }

            if (!code.Contains("."))
                code = IPExtensions.DecodeIpAddress(codeInt);

            

            CurrentClient.Connected += OnConnectToP2PServer;

            CurrentClient.Connect($"{code}:{port}", 5, 0, null, false);

            IsConnecting = true;
        }

        /// <summary>
        /// Connects to a Public Lobby based on the <paramref name="hostId"/> given.
        /// </summary>
        /// <param name="hostId"></param>
        public static void JoinPublicLobby(ushort hostId)
        {
            string serverIp = RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.GetValue();
            if (string.IsNullOrEmpty(serverIp)) 
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "No Server IP",
                    showTitleOnPopup = true,
                    message = $"You have no Public Lobby IP to join! Add one in Riptide Settings!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.ERROR
                });
            }

            if (IsConnecting)
            {

            }
        }

        private static void OnConnectToP2PServer(object sender, EventArgs e)
        {
            CurrentClient.Connected -= OnConnectToP2PServer;

            IsConnecting = false;

            CurrentClient.TimeoutTime = 30000;
            CurrentClient.Connection.CanQualityDisconnect = false;
            
            PlayerIdManager.SetLongId(CurrentClient.Id);

            ConnectionSender.SendConnectionRequest();

            InternalLayerHelpers.OnUpdateLobby();
        }

        /// <summary>
        /// Warns the client that a connection has failed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnConnectionFail(object sender, ConnectionFailedEventArgs e)
        {
            IsConnecting = false;

            FusionNotifier.Send(new FusionNotification()
            {
                title = "Connection Failed",
                showTitleOnPopup = true,
                message = $"Failed to connect to server! Is the server running?",
                isMenuItem = false,
                isPopup = true,
                popupLength = 5f,
                type = NotificationType.ERROR
            });
        }

        public static void OnDisconnectFromServer(object sender, DisconnectedEventArgs args)
        {
            InternalServerHelpers.OnDisconnect();
        }

        /// <summary>
        /// Calls a handler when a message is received based on its MessageId.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args"></param>
        public static void OnMessageReceived(object obj, MessageReceivedEventArgs args)
        {
            switch (args.MessageId)
            {
                case MessageTypes.FusionMessage:
                    {
                        FusionMessageHandler.ReadMessage(args.Message.GetBytes());
                        break;
                    }
            }
        }
    }
}
