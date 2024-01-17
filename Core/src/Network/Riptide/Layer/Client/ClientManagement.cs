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
using LabFusion.Riptide.Messages;
using System.Net;

namespace LabFusion.Riptide
{
    public class ClientManagement
    {
        public static Client CurrentClient = new();
        public static Client PublicLobbyClient = new();

        public static bool IsConnected => CurrentClient.IsConnected || PublicLobbyClient.IsConnected && RiptideNetworkLayer.HostId != 0;
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
                    showTitleOnPopup = false,
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
            switch (RiptidePreferences.LocalServerSettings.ServerType.GetValue())
            {
                case ServerTypes.P2P:
                    {
                        InternalServerHelpers.OnDisconnect();
                        break;
                    }
                case ServerTypes.Public:
                    {
                        if (RiptideNetworkLayer.HostId != 0)
                            InternalServerHelpers.OnDisconnect();
                        break;
                    }
            }
        }

        /// <summary>
        /// Connects to a Public Lobby based on the <paramref name="hostId"/> given.
        /// </summary>
        /// <param name="hostId"></param>
        public static void JoinPublicLobby(ushort hostId)
        {
            if (hostId == PublicLobbyClient.Id)
                return;

#if DEBUG
            FusionLogger.Log($"Joining Public Lobby with Host ID: {hostId}");
#endif

            string serverIp = RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.GetValue();

            if (ServerManagement.CurrentServer.IsRunning)
                ServerManagement.CurrentServer.Stop();

            if (CurrentClient.IsConnected)
                CurrentClient.Disconnect();

            if (PublicLobbyClient.IsConnected)
                PublicLobbyClient.Disconnect();

            RiptidePreferences.LocalServerSettings.ServerType.SetValue(ServerTypes.Public);

            RiptideNetworkLayer.HostId = hostId;

            void OnConnect(object sender, EventArgs e)
            {
                PublicLobbyClient.Connected -= OnConnect;

                PlayerIdManager.SetLongId(PublicLobbyClient.Id);

                var msg = PublicLobbyMessages.CreateJoinPublicLobbyMessage(hostId);
                PublicLobbyClient.Send(msg);
            }

            PublicLobbyClient.Connected += OnConnect;
            PublicLobbyClient.Connect($"{serverIp}:6666", 5, 0, null, false);
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
                        Messages.FusionMessage.HandleClientFusionMessage(args.Message);
                        break;
                    }
                case MessageTypes.RequestLobbies:
                    {
                        RiptideNetworkLayer.LobbyManager.HandleRiptideMessage(args.Message);
                        break;
                    }
                case MessageTypes.CreateLobby:
                    {
                        ServerManagement.HandleCreateLobbyMessage();
                        break;
                    }
                case MessageTypes.LobbyPlayerDisconnect:
                    {
                        bool hostDisconnected = args.Message.GetBool();
                        ushort id = args.Message.GetUShort();

                        if (hostDisconnected)
                        {
                            NetworkHelper.Disconnect("Host Disconnected");
                        } else
                        {
                            // Make sure the user hasn't previously disconnected
                            if (PlayerIdManager.HasPlayerId(id))
                            {
                                // Update the mod so it knows this user has left
                                InternalServerHelpers.OnUserLeave(id);

                                // Send disconnect notif to everyone
                                ConnectionSender.SendDisconnect(id);
                            }
                        }
                        break;
                    }
                case MessageTypes.JoinLobby:
                    {
                        ConnectionSender.SendConnectionRequest();

                        InternalLayerHelpers.OnUpdateLobby();
                        break;
                    }
            }
        }
    }
}
