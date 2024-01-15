using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobbyHost.Types;
using LobbyHost.UI;
using LobbyHost.MessageHandlers;

namespace LobbyHost
{
    internal class Hooking
    {
        internal static void OnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
        {
            var lobby = Lobby.GetLobby(e.Client.Id);

            if (lobby == null)
                return;

            if (lobby.HostId == e.Client.Id)
            {
                Core.CurrentLobbies.Remove(lobby);
                TUIManager.RefreshUi($"Lobby with Host ID {lobby.HostId} closed.");
            } else
            {

                TUIManager.RefreshUi($"Client with ID {e.Client.Id} disconnected from lobby with Host ID {lobby.HostId}");
            }
        }

        internal static void OnClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            e.Client.TimeoutTime = 30000;
            e.Client.CanQualityDisconnect = false;
        }

        internal static void OnMessageReceived(object? sender, MessageReceivedEventArgs e)
        {
            switch (e.MessageId)
            {
                case MessageTypes.RequestLobbies:
                    {
                        LobbyMessageHandler.HandleRequestLobbies(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.CreateLobby:
                    {
                        LobbyMessageHandler.HandleCreateLobby(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.DeleteLobby:
                    {
                        LobbyMessageHandler.HandleDeleteLobby(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.UpdateLobby:
                    {
                        LobbyMessageHandler.HandleUpdateLobby(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.PublicSendToServer:
                    {
                        FusionMessageHandler.HandleSendToServer(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.PublicSendFromServer:
                    {
                        FusionMessageHandler.HandleSendFromServer(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.PublicBroadcast:
                    {
                        FusionMessageHandler.HandleBroadcast(e.Message, e.FromConnection);
                        break;
                    }
            }
        }
    }
}
