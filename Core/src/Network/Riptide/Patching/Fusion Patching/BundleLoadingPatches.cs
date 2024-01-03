using HarmonyLib;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Utilities
{
    [HarmonyPatch(typeof(FusionBundleLoader))]
    public static class BundleLoadingPatches
    {
        // Load Tide bundle
        [HarmonyPostfix]
        [HarmonyPatch(nameof(FusionBundleLoader.OnBundleLoad))]
        public static void LoadTideBundle()
        {
            FusionLogger.Log("Loading Tide bundle");

            TideContentLoader.OnBundleLoad();
        }
    }
}
