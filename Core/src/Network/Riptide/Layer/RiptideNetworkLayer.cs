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

namespace LabFusion.Network.Riptide
{
    public class RiptideNetworkLayer : NetworkLayer
    {
        public ServerTypes CurrentServerType = ServerTypes.None;
        private Server CurrentServer = new();
        private Client CurrentClient = new();

        internal override bool IsClient => CurrentClient.IsConnected;
        internal override bool IsServer => CurrentServer.IsRunning;

        internal override string Title => "Riptide";

        internal override bool CheckSupported() => true;

        internal override bool CheckValidation() => true;

        internal override void OnCleanupLayer()
        {
            throw new NotImplementedException();
        }

        internal override void OnInitializeLayer()
        {
            throw new NotImplementedException();
        }

        internal override void OnLateInitializeLayer()
        {
            PlayerInfo.InitPlayerUsername();
        }

        internal override void OnSetupBoneMenu(MenuCategory category)
        {
            // Create the basic options
            CreateMatchmakingMenu(category);
            Keyboard.CreateKeyboard(category, "Test Keyboard", (text) =>
            {
                FusionNotification keyboardTest = new FusionNotification()
                {
                    title = "Keyboard Test",
                    showTitleOnPopup = true,
                    message = $"Keyboard Text: {text}",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 2f,
                };

                FusionNotifier.Send(keyboardTest);
            });
            BoneMenuCreator.CreateGamemodesMenu(category);
            BoneMenuCreator.CreateSettingsMenu(category);
            BoneMenuCreator.CreateNotificationsMenu(category);

#if DEBUG
            // Debug only (dev tools)
            BoneMenuCreator.CreateDebugMenu(category);
#endif
        }

        // Matchmaking menu
        private MenuCategory _serverInfoCategory;
        private MenuCategory _manualJoiningCategory;
        private void CreateMatchmakingMenu(MenuCategory category)
        {
            // Root category
            var matchmaking = category.CreateCategory("Matchmaking", Color.red);

            // Server making
            _serverInfoCategory = matchmaking.CreateCategory("Server Info", Color.white);
            CreateServerInfoMenu(_serverInfoCategory);

            // Manual joining
            _manualJoiningCategory = matchmaking.CreateCategory("Manual Joining", Color.white);
            CreateManualJoiningMenu(_manualJoiningCategory);
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

        private FunctionElement _targetServerElement;
        private void CreateManualJoiningMenu(MenuCategory category)
        {
            category.CreateFunctionElement("Join Server", Color.white, OnClickConnectToServer);
            _targetServerElement = category.CreateFunctionElement("Server ID:", Color.white, null);
        }

        private void OnClickConnectToServer()
        {

        }

        internal override void OnUpdateLobby()
        {
            throw new NotImplementedException();
        }

        internal override void StartServer()
        {
            CurrentServer.Start(7777, 256);

            CurrentClient.Connected += OnConnect;
        }

        private void OnConnect(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        internal override void Disconnect(string reason = "")
        {
            throw new NotImplementedException();
        }
    }
}
