using BoneLib.BoneMenu.Elements;
using BoneLib.BoneMenu.UI;
using BoneLib.BoneMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using HarmonyLib;
using LabFusion.Utilities;
using System.Configuration;
using LabFusion.UI;

namespace LabFusion.Network.Riptide.Utilities
{
    public class Keyboard
    {
        public static List<Keyboard> Keyboards;

        public Action<string> OnEnter;
        public MenuCategory Category;
        public GameObject KeyboardObject;
        public static void CreateKeyboard(BoneLib.BoneMenu.Elements.MenuCategory category, string name, Action<string> onEnter)
        {
            Keyboard keyboard = new Keyboard();

            MenuCategory keyboardCategory = category.CreateCategory(name, Color.cyan);

            if (keyboardCategory != null)
                keyboard.Category = keyboardCategory;
            else
            {
                FusionLogger.Error($"Failed to create keyboard category for {name}");
            }

            keyboard.OnEnter = onEnter;

            GameObject keyboardObject = GameObject.Instantiate(FusionContentLoader.KeyboardPrefab);
            var canvas = keyboardObject.transform.FindChild("Canvas");
            if (canvas == null)
            {
                FusionLogger.Error("Canvas is null!");
                return;
            }

            var keyboardCanvas = canvas.gameObject.AddComponent<KeyboardCanvas>();
            keyboardCanvas.Keyboard = keyboard;
            keyboardCanvas.SetupReferences();

            Keyboards.Add(keyboard);
        }

        [HarmonyPatch(typeof(UIManager), "OnCategoryUpdated")]
        public class CategoryUpdatePatch
        {
            public static void Postfix(UIManager __instance, MenuCategory category)
            {
                bool isKeyboardMenu = false;
                Keyboard keyboard = null;
                foreach (var obj in Keyboards)
                {
                    if (obj.Category == category)
                    {
                        isKeyboardMenu = true;
                        keyboard = obj;
                    }
                }
                if (isKeyboardMenu)
                {
                    __instance.MainPage.transform.Find("ScrollDown").gameObject.SetActive(false);
                    __instance.MainPage.transform.Find("ScrollUp").gameObject.SetActive(false);
                    __instance.MainPage.transform.Find("Return").gameObject.SetActive(false);

                    if (keyboard.KeyboardObject == null)
                    {
                        FusionLogger.Error("Keyboard Object is null!");
                    }
                    else
                    {
                        keyboard.KeyboardObject.transform.parent = __instance.MainPage.transform;
                        keyboard.KeyboardObject.transform.localPosition = Vector3.forward;
                        keyboard.KeyboardObject.transform.localRotation = Quaternion.identity;
                        keyboard.KeyboardObject.transform.localScale = Vector3.one;
                        keyboard.KeyboardObject.SetActive(true);
                    }
                }
                else if (category == MenuManager.RootCategory)
                {
                    __instance.MainPage.transform.Find("ScrollDown").gameObject.SetActive(true);
                    __instance.MainPage.transform.Find("ScrollUp").gameObject.SetActive(true);
                    __instance.MainPage.transform.Find("Return").gameObject.SetActive(true);
                    foreach (var obj in Keyboards)
                    {
                        obj.KeyboardObject.SetActive(false);
                    }
                }
            }
        }
    }
}
