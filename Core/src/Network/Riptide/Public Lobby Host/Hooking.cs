using Riptide;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobbyHost.Types;
using LobbyHost.UI;
using LobbyHost.MessageHandlers;
using static LobbyHost.MessageHandlers.FusionMessageHandler;
using Microsoft.VisualBasic;

namespace LobbyHost
{
    internal class Hooking
    {
        internal static void OnClientDisconnected(object? sender, ServerDisconnectedEventArgs e)
        {
            var lobby = Lobby.GetLobby(e.Client.Id);

            lobby.OnDisconnect(e.Client);
        }

        internal static void OnClientConnected(object? sender, ServerConnectedEventArgs e)
        {
            e.Client.TimeoutTime = 20000;
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
                case MessageTypes.UpdateLobby:
                    {
                        LobbyMessageHandler.HandleUpdateLobby(e.Message, e.FromConnection);
                        break;
                    }
                case MessageTypes.PublicSendToServer:
                    {
                        byte[] data = e.Message.GetBytes();
                        ushort id = e.Message.GetUShort();

                        var msg = CreateFusionMessage(data, e.Message.SendMode);
                        msg.AddBool(true);
                        Core.Server.Send(msg, id);

                        TUIManager.RefreshUi($"Got PublicSendToServer message from {e.FromConnection.Id} to {id}");
                        break;
                    }
                case MessageTypes.PublicSendFromServer:
                    {
                        byte[] data = e.Message.GetBytes();
                        ushort id = e.Message.GetUShort();
                        bool isHost = e.Message.GetBool();

                        var msg = CreateFusionMessage(data, e.Message.SendMode);
                        msg.AddBool(isHost);
                        Core.Server.Send(msg, id);

                        TUIManager.RefreshUi($"Got SendFromServer message from {e.FromConnection.Id} to {id}");
                        break;
                    }
                case MessageTypes.PublicBroadcast:
                    {
                        byte[] data = e.Message.GetBytes();
                        bool isHost = e.Message.GetBool();
                        ushort id = e.Message.GetUShort();

                        if (isHost)
                        {
                            var lobby = Lobby.GetHostLobby(e.FromConnection.Id);
                            var msg = CreateFusionMessage(data, e.Message.SendMode);
                            msg.AddBool(false);

                            foreach (var connection in lobby.Clients)
                            {
                                Core.Server.Send(msg, connection.Id, false);
                            }

                            msg.Release();
                        }
                        else
                        {
                            var msg = CreateFusionMessage(data, e.Message.SendMode);
                            msg.AddBool(true);
                            Core.Server.Send(msg, id);
                        }

                        TUIManager.RefreshUi($"Got Broadcast message from {e.FromConnection.Id}");
                        break;
                    }
                case MessageTypes.JoinLobby:
                    {
                        LobbyMessageHandler.HandleJoinLobby(e.Message, e.FromConnection);
                        break;
                    }
            }
        }
    }
}
