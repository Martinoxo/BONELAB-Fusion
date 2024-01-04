using HarmonyLib;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Patching
{
    [HarmonyPatch(typeof(FusionSceneManager))]
    public static class SceneManagerPatches
    {
        // Fix pesky font issue on Quest
        [HarmonyPostfix]
        [HarmonyPatch("Internal_UpdateLoadStatus")]
        public static void OnLevelUpdate()
        {
            Type type = typeof(LabFusion.Utilities.PersistentAssetCreator);
            MethodInfo methodInfo = type.GetMethod("CreateTextFont", BindingFlags.NonPublic | BindingFlags.Static);
            methodInfo.Invoke(null, null);
        }
    }
}
