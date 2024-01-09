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
using SLZ.Props;
using LabFusion.Riptide.Utilities;
using System.Windows.Forms;

namespace LabFusion.Riptide.BoneMenu
{
    public class Keyboard
    {
        public static List<Keyboard> Keyboards = new List<Keyboard>();

        public Action<string> OnEnter;
        public MenuCategory Category;
        public GameObject KeyboardObject;

        public static Keyboard CreateKeyboard(BoneLib.BoneMenu.Elements.MenuCategory category, string name, Action<string> onEnter)
        {
            Keyboard keyboard = new Keyboard();
            Keyboards.Add(keyboard);

            MenuCategory keyboardCategory = category.CreateCategory(name, Color.cyan);

            if (keyboardCategory != null)
                keyboard.Category = keyboardCategory;
            else
            {
                FusionLogger.Error($"Failed to create keyboard category for keyboard {name}");
            }

            keyboard.OnEnter = onEnter;

            GameObject keyboardObject = GameObject.Instantiate(TideContentLoader.KeyboardPrefab);
            keyboardObject.SetActive(true);
            if (keyboardObject == null )
            {
                FusionLogger.Error($"Keyboard is null for keyboard {name}!");
                return null;
            }
            var canvas = keyboardObject.transform.Find("Keyboard").Find("Canvas");
            if (canvas == null)
            {
                FusionLogger.Error($"Canvas is null for keyboard {name}!");
                return null;
            }

            var keyboardCanvas = canvas.gameObject.AddComponent<KeyboardCanvas>();
            keyboardCanvas.Keyboard = keyboard;
            keyboardCanvas.SetupReferences();
            keyboard.KeyboardObject = keyboardObject;

            keyboardObject.SetActive(false);

            return keyboard;
        }

        [HarmonyPatch(typeof(UIManager), nameof(UIManager.OnCategoryUpdated))]
        public class CategoryUpdatePatch
        {
            public static void Postfix(UIManager __instance, MenuCategory category)
            {
                foreach (var obj in Keyboards)
                {
                    if (obj.Category == category)
                    {
                        __instance.MainPage.transform.Find("ScrollDown").gameObject.SetActive(false);
                        __instance.MainPage.transform.Find("ScrollUp").gameObject.SetActive(false);
                        __instance.MainPage.transform.Find("Return").gameObject.SetActive(false);

                        if (obj.KeyboardObject == null)
                        {
                            obj.KeyboardObject = GameObject.Instantiate(TideContentLoader.KeyboardPrefab);
                            obj.KeyboardObject.SetActive(true);
                            if (obj.KeyboardObject == null)
                            {
                                FusionLogger.Error($"Keyboard is null!");
                                return;
                            }
                            var canvas = obj.KeyboardObject.transform.Find("Keyboard").Find("Canvas");
                            if (canvas == null)
                            {
                                FusionLogger.Error($"Canvas is null!");
                                return;
                            }

                            var keyboardCanvas = canvas.gameObject.AddComponent<KeyboardCanvas>();
                            keyboardCanvas.Keyboard = obj;
                            keyboardCanvas.SetupReferences();

                            obj.KeyboardObject.transform.parent = __instance.MainPage.transform;
                            obj.KeyboardObject.transform.localPosition = new Vector3 (0, 0, 40);
                            obj.KeyboardObject.transform.localRotation = Quaternion.identity;
                            obj.KeyboardObject.transform.localScale = new Vector3 (180, 180, 180);
                        } else
                        {
                            obj.KeyboardObject.SetActive(true);
                            obj.KeyboardObject.transform.parent = __instance.MainPage.transform;
                            obj.KeyboardObject.transform.localPosition = new Vector3(0, 0, 40);
                            obj.KeyboardObject.transform.localRotation = Quaternion.identity;
                            obj.KeyboardObject.transform.localScale = new Vector3(180, 180, 180);
                        }

                        break;
                    }
                    else
                    {
                        __instance.MainPage.transform.Find("ScrollDown").gameObject.SetActive(true);
                        __instance.MainPage.transform.Find("ScrollUp").gameObject.SetActive(true);
                        __instance.MainPage.transform.Find("Return").gameObject.SetActive(true);

                        if (obj.KeyboardObject != null)
                            obj.KeyboardObject.SetActive(false);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.SelectCategory))]
        public class CategorySelectPatch
        {
            public static void Postfix(MenuCategory category)
            {
                foreach (var keyboard in Keyboards)
                {
                    if (keyboard.KeyboardObject == null)
                        return;

                    if (keyboard.Category == category)
                        keyboard.KeyboardObject.SetActive(true);
                    else
                        keyboard.KeyboardObject.SetActive(false);
                }
            }
        }
    }
}
