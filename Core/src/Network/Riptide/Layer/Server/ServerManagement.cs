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

namespace LabFusion.Riptide
{
    public class ServerManagement
    {
        public static Server CurrentServer = new();

        public static void StartServer()
        {
            if (CurrentServer.IsRunning)
                CurrentServer.Stop();

            CurrentServer.Start(FusionPreferences.ClientSettings.ServerPort.GetValue(), 256, 0, false);

            ClientManagement.CurrentClient.Connected += OnConnectToP2PServer;
            ClientManagement.CurrentClient.Connect(
                $"127.0.0.1:{FusionPreferences.ClientSettings.ServerPort.GetValue()}", 5, 0, null, false);
        }

        private static void OnConnectToP2PServer(object sender, EventArgs e)
        {
            ClientManagement.CurrentClient.Connected -= OnConnectToP2PServer;

            ClientManagement.CurrentClient.TimeoutTime = 30000;
            ClientManagement.CurrentClient.Connection.CanQualityDisconnect = false;
            
            PlayerIdManager.SetLongId(ClientManagement.CurrentClient.Id);
            
            // Call server setup
            InternalServerHelpers.OnStartServer();

            InternalLayerHelpers.OnUpdateLobby();
        }

        // Hooks
        public static void OnClientDisconnect(object sender, ServerDisconnectedEventArgs e)
        {
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

        public static void OnClientConnect(object obj, ServerConnectedEventArgs args)
        {
            args.Client.CanQualityDisconnect = false;
        }
        
        public static void OnMessageReceived(object obj, MessageReceivedEventArgs args)
        {
            FusionMessageHandler.ReadMessage(args.Message.GetBytes(), true);
        }
    }
}
