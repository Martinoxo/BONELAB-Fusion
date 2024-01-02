using BoneLib.BoneMenu.Elements;
using HarmonyLib;
using LabFusion.BoneMenu;
using LabFusion.Data;
using LabFusion.Extensions;
using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using SLZ.Bonelab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Riptide.Patching
{
    [HarmonyPatch(typeof(BoneMenuCreator))]
    public static class PlayerListPatches
    {
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BoneMenuCreator.RefreshPlayerList))]
        public static void ModifyPlayerMenu()
        {
            if (NetworkLayerDeterminer.LoadedTitle != "Riptide")
                return;

            List<MenuCategory> playerMenus = BoneLib.BoneMenu.MenuManager.GetCategory("Player List").Elements.Where(x => x is MenuCategory).Cast<MenuCategory>().ToList();

            foreach (var menu in playerMenus)
            {
                var moderationCategory = BoneLib.BoneMenu.MenuManager.GetCategory("Moderation");
                var banElement = moderationCategory.Elements.FirstOrDefault(x => x.Name == "Ban");

                moderationCategory.Elements.Remove(banElement);
            }
        }
    }
}
