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
using LabFusion.Preferences;
using Riptide.Utils;
using System.Reflection;
using LabFusion.Riptide.Voice;
using LabFusion.Senders;
using LabFusion.SDK.Gamemodes;
using LabFusion.Riptide.Preferences;
using LabFusion.Grabbables;
using SLZ.Interaction;
using SLZ.Marrow.Data;
using SLZ.Props;

namespace LabFusion.Riptide
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        public static readonly string TideFusionPath = $"{MelonUtils.UserDataDirectory}/TideFusion";

        public static ServerTypes CurrentServerType = ServerTypes.None;

        internal override bool IsClient => CheckIsClient();
        internal override bool IsServer => CheckIsServer();

        private INetworkLobby _currentLobby;
        internal override INetworkLobby CurrentLobby => _currentLobby;
        private readonly RiptideVoiceManager _voiceManager = new();
        internal override IVoiceManager VoiceManager => _voiceManager;

        internal override string Title => "Riptide";

        internal override bool CheckSupported() => true;

        internal override bool CheckValidation()
        {
            return true;
        }

        internal override void OnInitializeLayer()
        {
            if (!System.IO.Directory.Exists(TideFusionPath))
                System.IO.Directory.CreateDirectory(TideFusionPath);

#if DEBUG
            RiptideLogger.Initialize(MelonLogger.Msg, true);
#endif
            RiptidePreferences.OnInitializePreferences();

            Message.MaxPayloadSize = 30000;
            HookRiptideEvents();
            
            FusionLogger.Log("Initialized Riptide layer");
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerInfo.InitializeUsername();
            PlayerInfo.InitializePlayerIPAddress();

            CurrentServer.TimeoutTime = 30000;
        }

        private void HookRiptideEvents()
        {
            // Riptide Hooks
            CurrentServer.ClientDisconnected += OnClientDisconnect;
            CurrentServer.ClientConnected += OnClientConnect;
            CurrentClient.Disconnected += OnDisconnectFromServer;
            CurrentClient.ConnectionFailed += OnConnectionFail;

            // Riptide Messages
            CurrentServer.MessageReceived += ServerManagement.OnMessageReceived;
            CurrentClient.MessageReceived += ClientManagement.OnMessageReceived;

            // Add Server Hooks
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
        }

        private void OnPlayerJoin(PlayerId id)
        {
            if (!id.IsSelf)
                VoiceManager.GetVoiceHandler(id);

            OnUpdateLobby();
        }

        private void OnPlayerLeave(PlayerId id)
        {
            VoiceManager.Remove(id);

            OnUpdateLobby();
        }

        private void OnDisconnect()
        {
            VoiceManager.RemoveAll();

            OnUpdateLobby();
        }

        // TODO: Add voice chat
        /*internal override void OnVoiceChatUpdate()
        {
            if (NetworkInfo.HasServer)
            {
                if (!VoiceHelper.IsVoiceEnabled)
                {
                    if (Microphone.IsRecording(null))
                    {
                        UnityVoiceIntegration.StopMicrophone();
                    }
                    return;
                }

                if (!Microphone.IsRecording(UnityVoiceIntegration._microphone))
                { 
                    UnityVoiceIntegration.StartMicrophone();
                }


                byte[] voiceData = UnityVoiceIntegration.GetVoiceData();
                if (voiceData != null)
                {
                    voiceData = UnityVoiceIntegration.Compress(voiceData);
                    PlayerSender.SendPlayerVoiceChat(voiceData);
                } else
                {
                    FusionLogger.Log("No voice data to send");
                }

                // Update the manager
                VoiceManager.Update();
            }
            else
            {
                // Disable voice recording
                UnityVoiceIntegration.StopMicrophone();
            }
        }

        internal override void OnVoiceBytesReceived(PlayerId id, byte[] bytes)
        {
            // If we are deafened, no need to deal with voice chat
            if (VoiceHelper.IsDeafened)
                return;

            var handler = VoiceManager.GetVoiceHandler(id);
            handler?.OnVoiceBytesReceived(bytes);
        }*/

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

        private FunctionElement _activeMicrophoneElement;
        private MenuCategory _microphoneSettingsCategory;
        private MenuCategory _microphoneCategory;
        private List<FunctionElement> _microphoneElements = new List<FunctionElement>();
        private void CreateRiptideMenu(MenuCategory category)
        {
            var riptideMenu = category.CreateCategory("Riptide Settings", Color.cyan);
            
            _microphoneSettingsCategory = riptideMenu.CreateCategory("Microphone Settings", Color.white);
            _activeMicrophoneElement = _microphoneSettingsCategory.CreateFunctionElement($"Active Microphone: \n{Microphone.devices[0]}", Color.white, null);
            _microphoneCategory = _microphoneSettingsCategory.CreateCategory("Change Microphone", Color.white);
            _microphoneCategory.CreateFunctionElement("Refresh Microphones", Color.cyan, () => OnClickRefreshMicrophones());
            foreach (var inputDevice in Microphone.devices)
            {
                var microphoneElement = _microphoneCategory.CreateFunctionElement(inputDevice, Color.white, () => OnSetMicrophone(inputDevice));
                _microphoneElements.Add(microphoneElement);
            }

#if DEBUG
            CreateRiptideDebugMenu(riptideMenu);
#endif
        }

        private void OnClickRefreshMicrophones()
        {
            foreach (var element in _microphoneElements)
            {
                _microphoneCategory.Elements.Remove(element);
                _microphoneElements.Remove(element);
            }

            foreach (var inputDevice in Microphone.devices)
            {
                var microphoneElement = _microphoneCategory.CreateFunctionElement(inputDevice, Color.white, () => OnSetMicrophone(inputDevice));
                _microphoneElements.Add(microphoneElement);
            }
        }

        private void OnSetMicrophone(string microphone)
        {
            _activeMicrophoneElement.SetName($"Active Microphone: \n{microphone}");

            BoneLib.BoneMenu.MenuManager.SelectCategory(_microphoneSettingsCategory);
        }

        private void CreateRiptideDebugMenu(MenuCategory category)
        {
            var debugMenu = category.CreateCategory("Debug", Color.red);
        }

        private FunctionElement _createServerElement;
        private Keyboard _serverPortKeyboard;

        private void CreateServerInfoMenu(MenuCategory category)
        {
            _createServerElement = category.CreateFunctionElement("Start P2P Server", Color.green, () => OnClickStartServer(), "P2P Servers REQUIRE that you Port Forward in order to host!");

            var p2pServerSettingsMenu = category.CreateCategory("Riptide Server Settings", Color.cyan);
            _serverPortKeyboard = Keyboard.CreateKeyboard(p2pServerSettingsMenu, $"Server Port:\n{RiptidePreferences.LocalServerSettings.ServerPort.GetValue()}", (port) => OnChangeServerPort(port));

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

            RiptidePreferences.LocalServerSettings.ServerPort.SetValue(result);
            _serverPortKeyboard.Category.SetName($"Server Port:\n{RiptidePreferences.LocalServerSettings.ServerPort.GetValue()}");
        }

        private MenuCategory _targetP2PCodeCategory;
        private MenuCategory _targetP2PPortCategory;
        private string _serverCodeToJoin;
        private ushort _serverPortToJoin = 7777;
        private void CreateP2PManualJoiningMenu(MenuCategory category)
        {
            category.CreateFunctionElement("Join Server", Color.green, () => ClientManagement.P2PJoinServer(_serverCodeToJoin, _serverPortToJoin));
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

        private bool CheckIsClient()
        {
            switch (CurrentServerType)
            {
                case ServerTypes.None:
                    return false;
                case ServerTypes.P2P:
                    return CurrentClient.IsConnected;
                case ServerTypes.Public:
                    if (PublicLobbyClient.IsConnected)
                        return true;
                    else
                        return false;
                case ServerTypes.Dedicated:
                    return false;
                default:
                    return false;
            }
        }

        private bool CheckIsServer()
        {
            switch (CurrentServerType)
            {
                case ServerTypes.None:
                    return false;
                case ServerTypes.P2P:
                    return CurrentServer.IsRunning;
                case ServerTypes.Public:
                    if (PublicLobbyClient.Id == ushort.Parse(CurrentLobby.GetMetadata("LobbyId")))
                        return true;
                    else
                        return false;
                case ServerTypes.Dedicated:
                    return false;
                default:
                    return false;
            }
        }

        internal override void OnUpdateLayer()
        {
            CurrentServer.Update();
            CurrentClient.Update();
            PublicLobbyClient.Update();
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
                _createServerElement.SetColor(Color.red);
                _fieldInfo.SetValue(_createServerElement, false);
            }
            else if (CurrentServer.IsRunning)
            {
                _createServerElement.SetName("Stop Server");
                _createServerElement.SetColor(Color.red);
                _fieldInfo.SetValue(_createServerElement, false);
            }
            else if (!CurrentClient.IsConnected)
            {
                _createServerElement.SetName("Start P2P Server");
                _createServerElement.SetColor(Color.green);
                _fieldInfo.SetValue(_createServerElement, true);
            }
        }

        internal override void StartServer() => ServerManagement.StartServer();

        // Am I insane for trying to implement three different networking systems into one layer? Maybe! Do I care? Nop.
        #region Fusion Messaging
        #region BROADCAST
        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            switch (CurrentServerType)
            {
                case ServerTypes.P2P:
                    if (IsServer)
                    {
                        CurrentServer.SendToAll(Messages.FusionMessage.CreateFusionMessage(message, channel));
                    }
                    else
                    {
                        CurrentClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel), true);
                    }
                    break;
                case ServerTypes.Public:
                    if (IsServer)
                    {
                        var msg = Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicBroadcast);
                        msg.AddUShort(PublicLobbyClient.Id);
                        PublicLobbyClient.Send(msg, false);
                    }
                    else
                    {
                        var msg = Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendToServer);
                        msg.AddUShort(ushort.Parse(CurrentLobby.GetMetadata("LobbyId")));
                        PublicLobbyClient.Send(msg, false);
                    }
                    break;
                case ServerTypes.Dedicated:
                    break;
            }
        }
        #endregion
        #region SENDTOSERVER
        internal override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            switch (CurrentServerType)
            {
                case ServerTypes.P2P:
                    CurrentClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel));
                    break;
                case ServerTypes.Public:
                    var msg = Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendToServer);
                    msg.AddUShort(ushort.Parse(CurrentLobby.GetMetadata("LobbyId")));
                    PublicLobbyClient.Send(msg, false);
                    break;
                case ServerTypes.Dedicated:
                    break;
            }
        }
        #endregion
        #region SENDFROMSERVER
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
            switch (CurrentServerType)
            {
                case ServerTypes.P2P:
                    if (IsServer)
                    {
                        Connection client;
                        if (userId == PlayerIdManager.LocalLongId)
                        {
                            CurrentServer.Send(Messages.FusionMessage.CreateFusionMessage(message, channel), (ushort)PlayerIdManager.LocalLongId);
                        }
                        else if (CurrentServer.TryGetClient((ushort)userId, out client))
                        {
                            CurrentServer.Send(Messages.FusionMessage.CreateFusionMessage(message, channel), client, true);
                        }
                    }
                    break;
                case ServerTypes.Public:
                    if (IsServer)
                    {
                        var msg = Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendFromServer);
                        msg.AddUShort((ushort)userId);
                        PublicLobbyClient.Send(msg, false);
                    }
                    break;
                case ServerTypes.Dedicated:
                    break;
            }
        }
        #endregion
        #endregion

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
