using BoneLib.BoneMenu.Elements;
using LabFusion.BoneMenu;
using LabFusion.Riptide.Utilities;
using LabFusion.Utilities;
using SLZ.Marrow.Warehouse;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UnityEngine;
using UnityEngine.Playables;

namespace LabFusion.Riptide.BoneMenu
{
    public class ServerListingCategory
    {
        private MenuCategory _category;

        private string _nameToEnter = "";
        private string _codeToEnter = "";
        private int _portToEnter = 7777;

        private Keyboard _nameKeyboard;
        private Keyboard _codeKeyboard;
        private Keyboard _portKeyboard;

        private List<MenuCategory> _listingCategories = new List<MenuCategory>();
        
        public static void CreateServerListingCategory(MenuCategory category)
        {
            ServerListingCategory serverListingCategory = new();
            serverListingCategory._category = category.CreateCategory("Server Listings", Color.white);

            var createCategory = serverListingCategory._category.CreateCategory("Create Listing", Color.green);
            
            serverListingCategory._nameKeyboard = Keyboard.CreateKeyboard(createCategory, $"Edit Name:\n{serverListingCategory._nameToEnter}", (name) => serverListingCategory.OnEnterName(name));
            serverListingCategory._codeKeyboard = Keyboard.CreateKeyboard(createCategory, $"Edit Code:\n{serverListingCategory._codeToEnter}", (code) => serverListingCategory.OnEnterCode(code));
            serverListingCategory._portKeyboard = Keyboard.CreateKeyboard(createCategory, $"Edit Port:\n{serverListingCategory._portToEnter}", (port) => serverListingCategory.OnEnterPort(port));

            createCategory.CreateFunctionElement("Done", Color.green, () => serverListingCategory.OnClickDone());

            serverListingCategory.CreateServerList(serverListingCategory._category);
        }

        private void OnEnterName(string name)
        {
            _nameToEnter = name;
            _nameKeyboard.Category.SetName($"Edit Name:\n" +
                                           $"{name}");
        }

        private void OnEnterCode(string code)
        {
            _codeToEnter = code;
            _codeKeyboard.Category.SetName($"Edit Code:\n" +
                                           $"{code}");
        }

        private void OnEnterPort(string port)
        {
            if (!int.TryParse(port, out int result) || result <= 1024 || result >= 65535)
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

            _portToEnter = result;
            _portKeyboard.Category.SetName($"Edit Port:\n" +
                                           $"{result}");
        }

        private void OnClickDone()
        {
            ServerListData data = new();
            data.Name = _nameToEnter;
            data.ServerCode = _codeToEnter;
            data.Port = _portToEnter;
            ServerListSaving.SaveServerList(data);

            foreach (var category in _listingCategories)
            {
                _category.Elements.Remove(category);
                _listingCategories.Remove(category);
            }

            CreateServerList(_category);

            BoneLib.BoneMenu.MenuManager.SelectCategory(_category);

            _nameToEnter = "";
            _codeToEnter = "";
            _portToEnter = 7777;

            _nameKeyboard.Category.SetName($"Edit Name:\n{_nameToEnter}");
            _codeKeyboard.Category.SetName($"Edit Code:\n{_codeToEnter}");
            _portKeyboard.Category.SetName($"Edit Port:\n{_portToEnter}");
        }

        private void OnClickDelete(ServerListData data, MenuCategory category)
        {
            System.IO.File.Delete(data.ServerListDataPath);
            _category.Elements.Remove(category);
            
            BoneLib.BoneMenu.MenuManager.SelectCategory(_category);
        }

        private void CreateServerList(MenuCategory category)
        {
            foreach (var listing in ServerListSaving.LoadServerList())
            {
                var listingCategory = category.CreateCategory(listing.Name, Color.white);
                var infoPanel = listingCategory.CreateSubPanel("Show Server Info", Color.yellow);
                infoPanel.CreateFunctionElement($"Server Code:\n{listing.ServerCode}", Color.white, null);
                infoPanel.CreateFunctionElement($"Server Port:\n{listing.Port}", Color.white, null);
                infoPanel.CreateFunctionElement("Delete Listing", Color.red, () => OnClickDelete(listing, listingCategory));
                
                listingCategory.CreateFunctionElement("Connect to Server", Color.green, null);

                _listingCategories.Add(listingCategory);
            }
        }
    }
}