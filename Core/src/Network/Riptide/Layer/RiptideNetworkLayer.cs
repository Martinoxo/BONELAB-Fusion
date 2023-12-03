using LabFusion.Network.Riptide.Utilities;
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
using static LabFusion.Network.Riptide.ServerManagement;
using static LabFusion.Network.Riptide.ClientManagement;
using MelonLoader;
using LabFusion.Network.Riptide.BoneMenu;

namespace LabFusion.Network.Riptide
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        public static readonly string TideFusionPath = $"{MelonUtils.UserDataDirectory}/TideFusion";

        public ServerTypes CurrentServerType = ServerTypes.None;

        internal override bool IsClient => _currentClient.IsConnected;
        internal override bool IsServer => _currentServer.IsRunning;

        internal override string Title => "Riptide";

        internal override bool CheckSupported() => true;

        internal override bool CheckValidation() => true;

        internal override void OnInitializeLayer()
        {
            if (!System.IO.Directory.Exists(TideFusionPath))
                System.IO.Directory.CreateDirectory(TideFusionPath);
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerInfo.InitPlayerUsername();
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
        private MenuCategory _p2pServerListCategory;

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

            ServerListing.CreateServerListingCategory(_p2pMatchmakingCategory);
        }

        private FunctionElement _createServerElement;

        private void CreateServerInfoMenu(MenuCategory category)
        {
            _createServerElement = category.CreateFunctionElement("Start Server", Color.white, OnClickStartServer);



            BoneMenuCreator.PopulateServerInfo(category);
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
                StartServer();
            }
        }

        private MenuCategory _targetP2PServerCategory;
        private string _serverCodeToJoin;
        private void CreateP2PManualJoiningMenu(MenuCategory category)
        {
            category.CreateFunctionElement("Join Server", Color.white, () => OnClickP2PJoin());
            _targetP2PServerCategory = Keyboard.CreateKeyboard(category, "Server Code:", OnChangeServerCode).Category;
        }

        private void OnChangeServerCode(string code)
        {
            _serverCodeToJoin = code;
            _targetP2PServerCategory.SetName($"Server Code:\n{_serverCodeToJoin}");
        }

        private void OnClickP2PJoin()
        {
            if (_serverCodeToJoin == null)
            {
                FusionNotification serverCodeWarning = new FusionNotification()
                {
                    title = "No Server Code",
                    showTitleOnPopup = true,
                    message = $"You have not entered a server code to join! Please click on the \"Server Code\" button to enter a server code!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 5f,
                    type = NotificationType.WARNING
                };
                FusionNotifier.Send(serverCodeWarning);
            }
        }

        internal override void OnUpdateLobby()
        {
            throw new NotImplementedException();
        }

        internal override void StartServer() => ServerManagement.StartServer();

        private void OnConnect(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Disconnect(string reason = "")
        {
            throw new NotImplementedException();
        }

        internal override void OnCleanupLayer()
        {
            throw new NotImplementedException();
        }
    }
}
