using BoneLib.BoneMenu.Elements;
using LabFusion.BoneMenu;
using LabFusion.Network.Riptide.Utilities;
using LabFusion.Utilities;
using SLZ.Marrow.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace LabFusion.Network.Riptide.BoneMenu
{
    public class ServerListing
    {
        private static MenuCategory _serverListingCategory = null;

        private static MenuCategory _createListingNameCategory;
        private static MenuCategory _createListingCodeCategory;
        private static MenuCategory _createListingPortCategory;

        private static string _serverListingName;
        private static string _serverListingCode;
        private static int _serverListingPort;

        public static MenuCategory CreateServerListingCategory(MenuCategory category = null)
        {
            if (_serverListingCategory == null)
                _serverListingCategory = category.CreateCategory("Server Listings", UnityEngine.Color.white);

            _serverListingCategory.Elements.Clear();

            _serverListingName = string.Empty;
            _serverListingCode = string.Empty;
            _serverListingPort = 7777;

            var createCategory = _serverListingCategory.CreateCategory("Create Server Listing", Color.cyan);
            _createListingNameCategory = Keyboard.CreateKeyboard(createCategory, $"Name:\n{_serverListingName}", OnChangeListingName).Category;
            _createListingCodeCategory = Keyboard.CreateKeyboard(createCategory, $"Code:\n{_serverListingCode}", OnChangeListingCode).Category;
            _createListingPortCategory = Keyboard.CreateKeyboard(createCategory, $"Port:\n{_serverListingPort}", OnChangeListingPort).Category;
            createCategory.CreateFunctionElement("Create Listing", Color.green, () => OnClickCreateListing(_serverListingName, _serverListingCode, _serverListingPort));

            foreach (var listingData in ServerListSaving.LoadServerList())
            {
#if DEBUG
                FusionLogger.Log($"Loading data: {listingData.Name}");
#endif
                CreateServerListing(listingData);
            }

            FusionLogger.Log("Finished creating server list!");

            return _serverListingCategory;
        }

        private static void OnClickCreateListing(string name, string code, int port = 7777)
        {
            if (name != string.Empty  || code != string.Empty)
            {
                FusionNotification serverListCategoryWarning = new FusionNotification()
                {
                    title = "Invalid Listing",
                    showTitleOnPopup = true,
                    message = $"This listing is missing either a Name or a Code field, make sure to add both!",
                    isMenuItem = false,
                    isPopup = true,
                    popupLength = 3f,
                    type = NotificationType.WARNING
                };
                FusionNotifier.Send(serverListCategoryWarning);

                return;
            }

            ServerListData serverListData = new ServerListData();
            serverListData.Name = name;
            serverListData.ServerCode = code;
            serverListData.Port = port;

            ServerListSaving.SaveServerList(serverListData);
            CreateServerListingCategory();
            BoneLib.BoneMenu.MenuManager.SelectCategory(_serverListingCategory);
        }

        private static void OnChangeListingName(string name)
        {
            _serverListingName = name;
            _createListingNameCategory.SetName($"Name:\n{_serverListingPort}");
        }
        private static void OnChangeListingCode(string code)
        {
            _serverListingCode = code;
            _createListingCodeCategory.SetName($"Code:\n{_serverListingPort}");
        }
        private static void OnChangeListingPort(string port)
        {
            _serverListingPort = int.Parse(port);
            _createListingPortCategory.SetName($"Port:\n{_serverListingPort}");
        }

        private static void CreateServerListing(ServerListData data)
        {
            if (_serverListingCategory == null)
            {
                return;
            }

            var category = _serverListingCategory.CreateCategory(data.Name, Color.white);
            category.CreateFunctionElement("Join Server", Color.green, () => ClientManagement.JoinServer(data.ServerCode));

            var subPanel = category.CreateSubPanel("Display Info", Color.yellow);
            subPanel.CreateFunctionElement($"Server Code:\n{data.ServerCode}", Color.white, null);
            subPanel.CreateFunctionElement($"Server Port:\n{data.Port}", Color.white, null);

            return;
        }
    }
}
