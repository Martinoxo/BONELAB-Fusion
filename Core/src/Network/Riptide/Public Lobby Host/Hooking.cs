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
            var args = Lobby.GetDisconnectArgs(e.Client);

            if (args.IsHost)
            {

                TUIManager.RefreshUi($"Lobby with Host ID {args.Lobby.LobbyInfo.HostId} closed.");
            } else
            {

                TUIManager.RefreshUi($"Client with ID {e.Client.Id} disconnected from lobby with Host ID {args.Lobby.LobbyInfo.HostId}");
            }
        }

        internal static void OnClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            
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
