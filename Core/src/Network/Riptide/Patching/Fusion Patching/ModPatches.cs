using HarmonyLib;
using LabFusion.BoneMenu;
using LabFusion.Network;
using LabFusion.Patching;
using LabFusion.Preferences;
using LabFusion.Riptide.Utilities;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Patching
{
    [HarmonyPatch(typeof(FusionMod))]
    public static class ModPatches
    {
        /// <summary>
        /// Removes Fusion's Auto-Updater warning on PC.
        /// </summary>
        /// <returns></returns>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FusionMod.OnLateInitializeMelon))]
        public static bool ModifyLateInitializeMelon()
        {
            ManualPatcher.PatchAll();
            InternalLayerHelpers.OnLateInitializeLayer();
            PersistentAssetCreator.OnLateInitializeMelon();
            PlayerAdditionsHelper.OnInitializeMelon();

            FusionPreferences.OnCreateBoneMenu();

            return false;
        }

        /// <summary>
        /// Loads RiptideNetworking and netstandard when Fusion is initialized.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(nameof(FusionMod.OnInitializeMelon))]
        public static void LoadAPIs()
        {
            NetstandardLoader.Load();
            RiptideNetworkingLoader.Load();
        }
    }
}
