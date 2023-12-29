using LabFusion.Riptide.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using BoneLib.BoneMenu.Elements;
using LabFusion.BoneMenu;
using UnityEngine;
using BoneLib;
using LabFusion.Utilities;
using static LabFusion.Riptide.ServerManagement;
using static LabFusion.Riptide.ClientManagement;
using MelonLoader;
using LabFusion.Riptide.BoneMenu;
using LabFusion.Network;
using LabFusion.Representation;
using System.Web.Services.Description;
using LabFusion.Preferences;
using Riptide.Utils;
using System.Reflection;

namespace LabFusion.Riptide
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        public static readonly string TideFusionPath = $"{MelonUtils.UserDataDirectory}/TideFusion";

        public ServerTypes CurrentServerType = ServerTypes.None;

        internal override bool IsClient => CurrentClient.IsConnected;
        internal override bool IsServer => CurrentServer.IsRunning;

        internal override string Title => "Riptide";

        internal override bool CheckSupported() => true;

        internal override bool CheckValidation() => true;

        internal override void OnInitializeLayer()
        {
            if (!System.IO.Directory.Exists(TideFusionPath))
                System.IO.Directory.CreateDirectory(TideFusionPath);

#if DEBUG
            RiptideLogger.Initialize(MelonLogger.Msg, true);
#endif
            
            HookRiptideEvents();
            
            FusionLogger.Log("Initialized Riptide layer");
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerInfo.InitializeUsername();
            PlayerInfo.InitializePlayerIPAddress();

            CurrentServer.TimeoutTime = 30000;
        }

        private static void HookRiptideEvents()
        {
            // Riptide Hooks
            CurrentServer.ClientDisconnected += OnClientDisconnect;
            CurrentServer.ClientConnected += OnClientConnect;
            CurrentClient.Disconnected += OnDisconnectFromServer;

            // Riptide Messages
            CurrentServer.MessageReceived += ServerManagement.OnMessageReceived;
            CurrentClient.MessageReceived += ClientManagement.OnMessageReceived;
        }

        internal override void OnSetupBoneMenu(MenuCategory category)
        {
            // Create the basic options
            CreateMatchmakingMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        // P2P Matchmaking
        private MenuCategory _p2pServerInfoCategory;
        private MenuCategory _p2pManualJoiningCategory;

        // Public Lobby Matchmaking (Future)
        private MenuCategory _publicMatchmakingCategory;
        private MenuCategory _publicServerInfoCategory;
        private MenuCategory _publicManualJoiningCategory;

        private void CreateMatchmakingMenu(MenuCategory category)
        {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            var _p2pMatchmakingCategory = matchmaking.CreateCategory("P2P Matchmaking", Color.white);

            // Server making
            _p2pServerInfoCategory = _p2pMatchmakingCategory.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_p2pServerInfoCategory);

            // Manual joining
            _p2pManualJoiningCategory = _p2pMatchmakingCategory.CreateCategory("Manual Joining", Color.white);
            CreateP2PManualJoiningMenu(_p2pManualJoiningCategory);

            // Server Listings
            ServerListingCategory.CreateServerListingCategory(_p2pMatchmakingCategory);
        }

        private FunctionElement _createServerElement;
        private Keyboard _serverPortKeyboard;

        private void CreateServerInfoMenu(MenuCategory category)
        {
            _createServerElement = category.CreateFunctionElement("Start Server", Color.white, () => OnClickStartServer(), "P2P Servers REQUIRE that you Port Forward in order to host! Make sure you have done this!");

            var p2pServerSettingsMenu = category.CreateCategory("Riptide Server Settings", Color.cyan);
            _serverPortKeyboard = Keyboard.CreateKeyboard(p2pServerSettingsMenu, $"Server Port:\n{FusionPreferences.ClientSettings.ServerPort.GetValue()}", (port) => OnChangeServerPort(port));

            category.CreateFunctionElement("Display Server Code", Color.white, () => OnDislayServerCode());

            BoneMenuCreator.PopulateServerInfo(category);
        }

        private void OnDislayServerCode()
        {
            FusionNotifier.Send(new FusionNotification()
            {
                isMenuItem = false,
                isPopup = true,
                showTitleOnPopup = true,
                popupLength = 20f,
                title = "Server Code",
                message = $"{IPExtensions.EncodeIpAddress(PlayerInfo.PlayerIpAddress)}",
                type = NotificationType.INFORMATION,
            });
        }

        private void OnClickStartServer()
        {
            // Is a server already running? Disconnect
            if (IsClient)
            {
                Disconnect();
            }
            // Otherwise, start a server
            else
            {
                ServerManagement.StartServer();
            }
        }

        private void OnChangeServerPort(string port)
        {
            if (!ushort.TryParse(port, out ushort result) || result <= 1024 || result >= 65535)
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    isMenuItem = false,
                    isPopup = true,
                    message = "Entered a Port which is incorrect!" +
                              "\nMake SURE to only input numbers and that the port range is between 1024 and 65535",
                    type = NotificationType.ERROR,
                });

                return;
            }

            FusionPreferences.ClientSettings.ServerPort.SetValue(result);
            _serverPortKeyboard.Category.SetName($"Server Port:\n{FusionPreferences.ClientSettings.ServerPort.GetValue()}");
        }

        private MenuCategory _targetP2PCodeCategory;
        private MenuCategory _targetP2PPortCategory;
        private string _serverCodeToJoin;
        private ushort _serverPortToJoin = 7777;
        private void CreateP2PManualJoiningMenu(MenuCategory category)
        {
            category.CreateFunctionElement("Join Server", Color.white, () => ClientManagement.P2PJoinServer(_serverCodeToJoin, _serverPortToJoin));
            _targetP2PCodeCategory = Keyboard.CreateKeyboard(category, "Server Code:", (code) => OnChangeJoinCode(code)).Category;
            _targetP2PPortCategory = Keyboard.CreateKeyboard(category, $"Server Port:\n{_serverPortToJoin}", (port) => OnChangeJoinPort(port)).Category;
        }

        private void OnChangeJoinCode(string code)
        {
            _serverCodeToJoin = code;
            _targetP2PCodeCategory.SetName($"Server Code:\n{_serverCodeToJoin}");
        }

        private void OnChangeJoinPort(string port)
        {
            if (!ushort.TryParse(port, out ushort result) || result <= 1024 || result >= 65535)
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    isMenuItem = false,
                    isPopup = true,
                    message = "Entered a Port which is incorrect!" +
                              "\nMake SURE to only input numbers and that the port range is between 1024 and 65535",
                    type = NotificationType.ERROR,
                });

                return;
            }

            _serverPortToJoin = result;
            _targetP2PPortCategory.SetName($"Edit Port:\n" +
                                           $"{result}");
        }

        internal override void OnUpdateLayer()
        {
            CurrentServer.Update();
            CurrentClient.Update();
        }

        internal override void OnUpdateLobby()
        {
            // Update bonemenu items
            OnUpdateCreateServerText();
        }

        private FieldInfo _fieldInfo = typeof(FunctionElement).GetField("_confirmer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private void OnUpdateCreateServerText()
        {
            if (CurrentClient.IsConnected && !CurrentServer.IsRunning)
            {
                _createServerElement.SetName("Disconnect");
                _fieldInfo.SetValue(_createServerElement, false);
            }
            else if (CurrentServer.IsRunning)
            {
                _createServerElement.SetName("Stop Server");
                _fieldInfo.SetValue(_createServerElement, false);
            }
            else if (!CurrentClient.IsConnected)
            {
                _createServerElement.SetName("Start P2P Server");
                _fieldInfo.SetValue(_createServerElement, true);
            }
        }

        internal override void StartServer() => ServerManagement.StartServer();

        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            if (IsServer)
            {
                CurrentServer.SendToAll(Riptide.Messages.FusionMessage.CreateFusionMessage(message, channel));
            }
            else
            {
                CurrentClient.Send(Riptide.Messages.FusionMessage.CreateFusionMessage(message, channel), true);
            }
        }

        internal override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            CurrentClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel));
        }

        internal override void SendFromServer(byte userId, NetworkChannel channel, FusionMessage message)
        {
            PlayerId playerId = PlayerIdManager.GetPlayerId(userId);
            if (playerId != null)
            {
                SendFromServer(playerId.LongId, channel, message);
            }
        }

        internal override void SendFromServer(ulong userId, NetworkChannel channel, FusionMessage message)
        {
            if (IsServer)
            {
                Connection client;
                if (userId == PlayerIdManager.LocalLongId)
                {
                    CurrentServer.Send(Riptide.Messages.FusionMessage.CreateFusionMessage(message, channel), (ushort)PlayerIdManager.LocalLongId);
                }
                else if (CurrentServer.TryGetClient((ushort)userId, out client))
                {
                    CurrentServer.Send(Riptide.Messages.FusionMessage.CreateFusionMessage(message, channel), client, true);
                }
            }
        }

        internal override void Disconnect(string reason = "")
        {
            // Make sure we are currently in a server
            if (!IsServer && !IsClient)
                return;

            if (IsClient)
                CurrentClient.Disconnect();

            if (IsServer)
                CurrentServer.Stop();

            OnUpdateLobby();
        }

        internal override void OnCleanupLayer()
        {
            Disconnect();
        }
    }
}
