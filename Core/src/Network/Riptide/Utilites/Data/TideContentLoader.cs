using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LabFusion.Riptide.Utilities
{
    public static class TideContentLoader
    {
        public static AssetBundle TideBundle { get; private set; }

        public static GameObject KeyboardPrefab { get; private set; }

        public static void OnBundleLoad()
        {
            TideBundle = FusionBundleLoader.LoadAssetBundle(ResourcePaths.TideBundle);

            if (TideBundle != null)
            {
                KeyboardPrefab = TideBundle.LoadPersistentAsset<GameObject>(ResourcePaths.KeyboardPrefab);
            }
        }
    }
}
