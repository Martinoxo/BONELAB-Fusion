using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Web.UI;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using Riptide;
using Unity.Collections;
using LabFusion.Riptide.Utilities;
using LabFusion.Riptide.Preferences;
using LabFusion.Utilities;
using LabFusion.Riptide.Messages;
using static LabFusion.Riptide.ClientManagement;
using System.Net;
using LabFusion.Data;

namespace LabFusion.Riptide
{
    public class ServerManagement
    {
        public static Server CurrentServer = new();

        public static void StartServer()
        {
            if (ClientManagement.IsConnecting)
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    showTitleOnPopup = false,
                    message = $"Cannot start server whilst connecting to a server! Either wait for the connection to fail, or stop connecting to a server!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.WARNING
                });
                return;
            }

            if (CurrentServer.IsRunning)
                CurrentServer.Stop();

            CurrentServer.Start(RiptidePreferences.LocalServerSettings.ServerPort.GetValue(), 256, 0, false);

			//CurrentClient.Connected += OnConnectToP2PServer;
			//CurrentClient.Connect(
			//    $"127.0.0.1:{RiptidePreferences.LocalServerSettings.ServerPort.GetValue()}", 5, 0, null, false);

			RiptidePreferences.LocalServerSettings.ServerType.SetValue(ServerTypes.P2P);
			PlayerIdManager.SetLongId(0);
			InternalServerHelpers.OnStartServer();
			InternalLayerHelpers.OnUpdateLobby();

			FusionNotifier.Send(new FusionNotification()
			{
				showTitleOnPopup = false,
				message = $"Hacky wacky client to server",
				isMenuItem = false,
				isPopup = true,
				popupLength = 4f,
				type = NotificationType.INFORMATION
			});
		}

        private static void OnConnectToP2PServer(object sender, EventArgs e)
        {
            CurrentClient.Connected -= OnConnectToP2PServer;

            RiptidePreferences.LocalServerSettings.ServerType.SetValue(ServerTypes.P2P);
            RiptideNetworkLayer.HostId = 0;

            CurrentClient.TimeoutTime = 15000;
            CurrentClient.Connection.CanQualityDisconnect = false;
            
            PlayerIdManager.SetLongId(CurrentClient.Id);
            
            // Call server setup
            InternalServerHelpers.OnStartServer();

            InternalLayerHelpers.OnUpdateLobby();

			FusionNotifier.Send(new FusionNotification()
			{
				showTitleOnPopup = false,
				message = $"OnConnectToP2PServer",
				isMenuItem = false,
				isPopup = true,
				popupLength = 4f,
				type = NotificationType.INFORMATION
			});
			return;
		}

        // Hooks
        public static void OnClientDisconnect(object sender, ServerDisconnectedEventArgs e)
        {
			FusionNotifier.Send(new FusionNotification()
			{
				showTitleOnPopup = false,
				message = $"OnClientDisconnect",
				isMenuItem = false,
				isPopup = true,
				popupLength = 4f,
				type = NotificationType.INFORMATION
			});

			var id = e.Client.Id;
            // Make sure the user hasn't previously disconnected
            if (PlayerIdManager.HasPlayerId(id))
            {
                // Update the mod so it knows this user has left
                InternalServerHelpers.OnUserLeave(id);

                // Send disconnect notif to everyone
                ConnectionSender.SendDisconnect(id);
            }
        }

        public static void OnClientConnect(object sender, ServerConnectedEventArgs e)
        {
			FusionNotifier.Send(new FusionNotification()
			{
				showTitleOnPopup = false,
				message = $"OnClientConnect",
				isMenuItem = false,
				isPopup = true,
				popupLength = 4f,
				type = NotificationType.INFORMATION
			});

			e.Client.CanQualityDisconnect = false;
            e.Client.TimeoutTime = 15000;

		    ushort id = e.Client.Id;
			if (!PlayerIdManager.HasPlayerId(id))
			{
                PlayerId playerId = PlayerIdManager.GetPlayerId(e.Client.Id);

                SerializedAvatarStats stats = new SerializedAvatarStats();

				InternalServerHelpers.OnUserJoin(playerId,false);
                ConnectionSender.SendPlayerJoin(playerId,CommonBarcodes.STRONG_BARCODE,stats);
			}
		}

        public static void CreatePublicLobby()
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
                return;
            }

            if (!IPAddress.TryParse(serverIp, out var ip))
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    title = "Invalid Server IP",
                    showTitleOnPopup = true,
                    message = $"The Public Lobby IP you entered is invalid!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.ERROR
                });
                return;
            }

            if (!PublicLobbyClient.IsConnected)
            {
                void OnConnect(object sender, EventArgs e)
                {
                    PublicLobbyClient.Connected -= OnConnect;
                    PublicLobbyClient.Send(PublicLobbyMessages.CreatePublicLobbyMessage());
                };

                PublicLobbyClient.Connected += OnConnect;
                PublicLobbyClient.Connect($"{serverIp}:6666", 5, 0, null, false);
            }
            else
            {
                PublicLobbyClient.Send(PublicLobbyMessages.CreatePublicLobbyMessage());
            }
        }

        internal static void HandleCreateLobbyMessage()
        {
            RiptideNetworkLayer.HostId = PublicLobbyClient.Id;

            PublicLobbyClient.TimeoutTime = 30000;
            PublicLobbyClient.Connection.CanQualityDisconnect = false;

            PlayerIdManager.SetLongId(PublicLobbyClient.Id);

            // Call server setup
            InternalServerHelpers.OnStartServer();

            InternalLayerHelpers.OnUpdateLobby();
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
                        Messages.FusionMessage.HandleServerFusionMessage(args.Message);
                        break;
                    }
            }
        }
    }
}
