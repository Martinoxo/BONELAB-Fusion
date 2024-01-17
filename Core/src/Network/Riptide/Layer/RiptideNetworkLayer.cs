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
using BoneLib.BoneMenu;
using System.Collections;
using System.Net;
using System.Windows.Forms.DataVisualization.Charting;

namespace LabFusion.Riptide
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        public static readonly string TideFusionPath = $"{MelonUtils.UserDataDirectory}/TideFusion";

        internal static ushort HostId = 0;

        internal override bool IsClient => CheckIsClient();
        internal override bool IsServer => CheckIsServer();

        private RiptideLobby _currentLobby = new RiptideLobby(new List<KeyValuePair<string, string>>(), 0);
        internal override INetworkLobby CurrentLobby => _currentLobby;
        private readonly RiptideVoiceManager _voiceManager = new();
        internal override IVoiceManager VoiceManager => _voiceManager;

        public static RiptideLobbyManager LobbyManager = new();

        internal override string Title => "Riptide";

        internal override bool CheckSupported() => true;

        internal override bool CheckValidation() => true;

        internal override bool IsFriend(ulong userId) => false;

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
            PublicLobbyClient.Disconnected += OnDisconnectFromServer;
            PublicLobbyClient.ConnectionFailed += OnPublicConnectionFail;

            // Riptide Messages
            CurrentServer.MessageReceived += ServerManagement.OnMessageReceived;
            CurrentClient.MessageReceived += ClientManagement.OnMessageReceived;
            PublicLobbyClient.MessageReceived += ClientManagement.OnMessageReceived;

            // Add Server Hooks
            MultiplayerHooking.OnPlayerJoin += OnPlayerJoin;
            MultiplayerHooking.OnPlayerLeave += OnPlayerLeave;
            MultiplayerHooking.OnDisconnect += OnDisconnect;
            MultiplayerHooking.OnServerSettingsChanged += OnUpdateLobby;
        }

        private void OnPublicConnectionFail(object sender, ConnectionFailedEventArgs e)
        {
            FusionNotifier.Send(new FusionNotification()
            {
                title = "Connection Failed",
                showTitleOnPopup = true,
                message = $"Failed to connect to Public Lobby Host! Is the Host running?",
                isMenuItem = false,
                isPopup = true,
                popupLength = 5f,
                type = NotificationType.ERROR
            });
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
            HostId = 0;

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
            CreateRiptideMenu(category);
            BoneMenuCreator.CreateGamemodesMenu(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        private FunctionElement _serverTypeElement;
        private FunctionElement _createServerElement;
        private Keyboard _serverPortKeyboard;
        private void CreateMatchmakingMenu(MenuCategory category)
        {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server info
            var serverInfo = matchmaking.CreateCategory("Server Info", Color.white);

            // Server Starting/Settings
            _createServerElement = serverInfo.CreateFunctionElement("Start Server", Color.green, () => OnClickStartServer(), "P2P Servers REQUIRE that you Port Forward in order to host!");
            _serverTypeElement = serverInfo.CreateFunctionElement($"Server Type: \n{RiptidePreferences.LocalServerSettings.ServerType.GetValue()}", Color.cyan, () => OnUpdateServerType(RiptidePreferences.LocalServerSettings.ServerType.GetValue()));
            RiptidePreferences.LocalServerSettings.ServerType.OnValueChanged += (v) => _serverTypeElement.SetName($"Server Type: \n{v}");
            var p2pServerSettingsMenu = serverInfo.CreateCategory("P2P Server Settings", Color.cyan);
            _serverPortKeyboard = Keyboard.CreateKeyboard(p2pServerSettingsMenu, $"Server Port:\n{RiptidePreferences.LocalServerSettings.ServerPort.GetValue()}", (port) => OnChangeServerPort(port));
            serverInfo.CreateFunctionElement("Display Server Code", Color.white, () => OnDislayServerCode());

            // Server Info
            BoneMenuCreator.PopulateServerInfo(serverInfo);

            // P2P Server
            CreateP2PJoiningMenu(matchmaking);

            // Public Lobbies
            CreatePublicLobbyCategory(matchmaking);
        }

        private void OnUpdateServerType(ServerTypes type)
        {
            if (IsClient)
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    isMenuItem = false,
                    isPopup = true,
                    message = "Cannot change server type whilst connected!",
                    type = NotificationType.ERROR,
                });
                return;
            }

            switch (type)
            {
                case ServerTypes.P2P:
                    RiptidePreferences.LocalServerSettings.ServerType.SetValue(ServerTypes.Public);
                    _serverTypeElement.SetName($"Server Type: \n{RiptidePreferences.LocalServerSettings.ServerType.GetValue()}");
                    break;
                case ServerTypes.Public:
                    RiptidePreferences.LocalServerSettings.ServerType.SetValue(ServerTypes.P2P);
                    _serverTypeElement.SetName($"Server Type: \n{RiptidePreferences.LocalServerSettings.ServerType.GetValue()}");
                    break;
            }

            OnUpdateCreateServerText();
        }

        private Keyboard _publicLobbyIpKeyboard;
        private FunctionElement _activeMicrophoneElement;
        private MenuCategory _microphoneSettingsCategory;
        private MenuCategory _microphoneCategory;
        private List<FunctionElement> _microphoneElements = new List<FunctionElement>();

        private void CreateRiptideMenu(MenuCategory category)
        {
            var riptideMenu = category.CreateCategory("Riptide Settings", Color.blue);

            _publicLobbyIpKeyboard = Keyboard.CreateKeyboard(riptideMenu, $"Public Lobby IP:\n{RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.GetValue()}", (ip) => OnChangePublicLobbyIp(ip, riptideMenu));

            /*_microphoneSettingsCategory = riptideMenu.CreateCategory("Microphone Settings", Color.white);
            _activeMicrophoneElement = _microphoneSettingsCategory.CreateFunctionElement($"Active Microphone: \n{Microphone.devices[0]}", Color.white, null);
            _microphoneCategory = _microphoneSettingsCategory.CreateCategory("Change Microphone", Color.white);
            _microphoneCategory.CreateFunctionElement("Refresh Microphones", Color.cyan, () => OnClickRefreshMicrophones());
            foreach (var inputDevice in Microphone.devices)
            {
                var microphoneElement = _microphoneCategory.CreateFunctionElement(inputDevice, Color.white, () => OnSetMicrophone(inputDevice));
                _microphoneElements.Add(microphoneElement);
            }*/

#if DEBUG
            CreateRiptideDebugMenu(riptideMenu);
#endif
        }

        private void OnChangePublicLobbyIp(string ip, MenuCategory categoryToSwitchTo)
        {
            if (!IPAddress.TryParse(ip, out var serverIp))
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

            if (!PublicLobbyClient.IsConnected && !PublicLobbyClient.IsConnecting)
            {
                RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.SetValue(ip);
                _publicLobbyIpKeyboard.Category.SetName($"Public Lobby IP:\n{RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.GetValue()}");
            } else
            {
                FusionNotifier.Send(new FusionNotification()
                {
                    showTitleOnPopup = false,
                    message = "Cannot change IP whilst PublicLobbyClient is connected!",
                    isMenuItem = false,
                    isPopup = true,
                });
            }

            MenuManager.SelectCategory(categoryToSwitchTo);
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

            MenuManager.SelectCategory(_microphoneSettingsCategory);
        }

        private void CreateRiptideDebugMenu(MenuCategory category)
        {
            var debugMenu = category.CreateCategory("Debug", Color.red);
        }

        private void CreateP2PJoiningMenu(MenuCategory category)
        {
            var p2pMenu = category.CreateCategory("P2P Joining", Color.white);

            CreateP2PManualJoiningMenu(p2pMenu);

            // Server Listings
            ServerListingCategory.CreateServerListingCategory(p2pMenu);
        }

        private MenuCategory _publicLobbiesCategory;
        private void CreatePublicLobbyCategory(MenuCategory category)
        {
            var publicLobbyCategory = category.CreateCategory("Public Joining", Color.white);

            // Manual Joining
            var manualJoining = publicLobbyCategory.CreateCategory("Manual Joining", Color.white);

            // Public lobbies list
            _publicLobbiesCategory = publicLobbyCategory.CreateCategory("Public Lobbies", Color.white);
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);
            _publicLobbiesCategory.CreateFunctionElement("Select Refresh to load servers!", Color.yellow, null);
        }

        private LobbySortMode _publicLobbySortMode = LobbySortMode.LEVEL;
        private bool _isPublicLobbySearching = false;

        private void Menu_RefreshPublicLobbies()
        {
            // Make sure we arent already searching
            if (_isPublicLobbySearching)
                return;

            // Clear existing lobbies
            _publicLobbiesCategory.Elements.Clear();
            _publicLobbiesCategory.CreateFunctionElement("Refresh", Color.white, Menu_RefreshPublicLobbies);
            _publicLobbiesCategory.CreateEnumElement("Sort By", Color.white, _publicLobbySortMode, (v) =>
            {
                _publicLobbySortMode = v;
                Menu_RefreshPublicLobbies();
            });

            MelonCoroutines.Start(CoAwaitLobbyListRoutine());
        }

        private IEnumerator CoAwaitLobbyListRoutine()
        {
            _isPublicLobbySearching = true;
            LobbySortMode sortMode = _publicLobbySortMode;

            // Fetch lobbies
            var task = LobbyManager.RequestLobbies();

            float timeTaken = 0f;

            while (!task.IsCompleted)
            {
                yield return null;
                timeTaken += TimeUtilities.DeltaTime;

                if (timeTaken >= 20f || !PublicLobbyClient.IsConnected && !PublicLobbyClient.IsConnecting)
                {
                    FusionNotifier.Send(new FusionNotification()
                    {
                        title = "Timed Out",
                        showTitleOnPopup = true,
                        message = "Timed out when requesting lobbies. Is the Public Lobby Host running?",
                        isMenuItem = false,
                        isPopup = true,
                    });
                    _isPublicLobbySearching = false;
                    yield break;
                }
            }

            var lobbies = task.Result;

            using (BatchedBoneMenu.Create())
            {
                foreach (var lobby in lobbies)
                {
                    // Hide lobby if it is our own
                    if (lobby.HostId == PublicLobbyClient.Id)
                        continue;

                    var info = LobbyMetadataHelper.ReadInfo(lobby);

                    if (Internal_CanShowLobby(info))
                    {
                        // Add to list
                        BoneMenuCreator.CreateLobby(_publicLobbiesCategory, info, lobby, sortMode);
                    }
                }
            }

            // Select the updated category
            MenuManager.SelectCategory(_publicLobbiesCategory);

            _isPublicLobbySearching = false;
        }

        private bool Internal_CanShowLobby(LobbyMetadataInfo info)
        {
            // Make sure the lobby is actually open
            if (!info.HasServerOpen)
                return false;

            // Decide if this server is too private
            switch (info.Privacy)
            {
                default:
                case ServerPrivacy.LOCKED:
                case ServerPrivacy.PRIVATE:
                    return false;
                case ServerPrivacy.PUBLIC:
                    return true;
                case ServerPrivacy.FRIENDS_ONLY:
                    return IsFriend(info.LobbyId);
            }
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
                switch (RiptidePreferences.LocalServerSettings.ServerType.GetValue())
                {
                    case ServerTypes.P2P:
                        ServerManagement.StartServer();
                        break;
                    case ServerTypes.Public:
                        ServerManagement.CreatePublicLobby();
                        break;
                }
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
            var manualJoining = category.CreateCategory("Manual Joining", Color.white);
            manualJoining.CreateFunctionElement("Join Server", Color.green, () => P2PJoinServer(_serverCodeToJoin, _serverPortToJoin));
            _targetP2PCodeCategory = Keyboard.CreateKeyboard(manualJoining, "Server Code:", (code) => OnChangeJoinCode(code)).Category;
            _targetP2PPortCategory = Keyboard.CreateKeyboard(manualJoining, $"Server Port:\n{_serverPortToJoin}", (port) => OnChangeJoinPort(port)).Category;
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
            if (CurrentClient == null || PublicLobbyClient == null)
                return false;

            return (CurrentClient.IsConnected || PublicLobbyClient.IsConnected && HostId != 0);
        }

        private bool CheckIsServer()
        {
            if (CurrentServer == null || PublicLobbyClient == null || HostId == 0)
                return false;

            switch (RiptidePreferences.LocalServerSettings.ServerType.GetValue())
            {
                case ServerTypes.P2P:
                    return CurrentServer.IsRunning;
                case ServerTypes.Public:
                    if (PublicLobbyClient.Id == HostId)
                        return true;
                    else
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
            // Make sure the lobby exists
            if (CurrentLobby == null)
            {
#if DEBUG
                FusionLogger.Warn("Tried updating the riptide lobby, but it was null!");
#endif
                return;
            }

            // Write active info about the lobby
            LobbyMetadataHelper.WriteInfo(CurrentLobby);

            // Update bonemenu items
            OnUpdateCreateServerText();
        }

        private FieldInfo _fieldInfo = typeof(FunctionElement).GetField("_confirmer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        private void OnUpdateCreateServerText()
        {
            try
            {
                if (_createServerElement == null)
                    return;

                if (IsClient && !IsServer)
                {
                    _createServerElement.SetName("Disconnect");
                    _createServerElement.SetColor(Color.red);
                }
                else if (IsServer)
                {
                    _createServerElement.SetName("Stop Server");
                    _createServerElement.SetColor(Color.red);
                }
                else if (!IsClient)
                {
                    _createServerElement.SetName("Start Server");
                    _createServerElement.SetColor(Color.green);
                }

                if (RiptidePreferences.LocalServerSettings.ServerType.GetValue() == ServerTypes.P2P)
                {
                    if (!IsClient)
                    {
                        _fieldInfo.SetValue(_createServerElement, true);
                    }
                }
                else
                    _fieldInfo.SetValue(_createServerElement, false);
            } catch (Exception ex)
            {
                FusionLogger.Error($"Failed to update Create Server text with reason: {ex.Message}");
            }
        }

        internal override void StartServer() => ServerManagement.StartServer();

        // Am I insane for trying to implement three different networking systems into one layer? Maybe! Do I care? Nop.
        #region Fusion Messaging
        #region BROADCAST
        internal override void BroadcastMessage(NetworkChannel channel, FusionMessage message)
        {
            switch (RiptidePreferences.LocalServerSettings.ServerType.GetValue())
            {
                case ServerTypes.P2P:
                    if (IsServer)
                    {
                        CurrentServer.SendToAll(Messages.FusionMessage.CreateFusionMessage(message, channel));
                    }
                    else
                    {
                        CurrentClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel));
                    }
                    break;
                case ServerTypes.Public:
                    if (IsServer)
                    {
                        PublicLobbyClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicBroadcast));
                    } else
                    {
                        PublicLobbyClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendToServer).AddUShort(HostId));
                    }
                    break;
            }
        }
        #endregion
        #region SENDTOSERVER
        internal override void SendToServer(NetworkChannel channel, FusionMessage message)
        {
            switch (RiptidePreferences.LocalServerSettings.ServerType.GetValue())
            {
                case ServerTypes.P2P:
                    CurrentClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel));
                    break;
                case ServerTypes.Public:
                    PublicLobbyClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendToServer).AddUShort(HostId));
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
            switch (RiptidePreferences.LocalServerSettings.ServerType.GetValue())
            {
                case ServerTypes.P2P:
                    if (IsServer)
                    {
                        if (userId == PlayerIdManager.LocalLongId)
                        {
                            FusionMessageHandler.ReadMessage(message.ToByteArray());
                        }
                        else if (CurrentServer.TryGetClient((ushort)userId, out var client))
                        {
                            CurrentServer.Send(Messages.FusionMessage.CreateFusionMessage(message, channel), client);
                        }
                    }
                    break;
                case ServerTypes.Public:
                    if (IsServer)
                    {
                        if (userId == PlayerIdManager.LocalLongId)
                        {
                            PublicLobbyClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendFromServer).AddUShort((ushort)userId).AddBool(true));
                        } else
                        {
                            PublicLobbyClient.Send(Messages.FusionMessage.CreateFusionMessage(message, channel, MessageTypes.PublicSendFromServer).AddUShort((ushort)userId).AddBool(false));
                        }
                    }
                    break;
            }
        }
        #endregion
        #endregion

        internal override void Disconnect(string reason = "")
        {
            {
                if (!IsServer && !IsClient)
                    return;

                if (IsClient)
                {
                    CurrentClient.Disconnect();
                    PublicLobbyClient.Disconnect();
                }

                if (IsServer && RiptidePreferences.LocalServerSettings.ServerType.GetValue() == ServerTypes.P2P)
                    CurrentServer.Stop();

                OnUpdateLobby();
            }
        }

        internal override void OnCleanupLayer()
        {
            Disconnect();
        }
    }
}
