using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Utilities
{
    /// <summary>
    /// Contains different paths to embedded resources within the TIDE layer.
    /// </summary>
    internal static class ResourcePaths
    {
        internal const string RiptideNetworkingPath = "LabFusion.Core.src.Network.Riptide.resources.lib.RiptideNetworking.dll";
        internal const string netstandardPath = "LabFusion.Core.src.Network.Riptide.resources.lib.netstandard.dll";

        public const string WindowsBundlePrefix = "LabFusion.Core.src.Network.Riptide.resources.bundles.StandaloneWindows64.";
        public const string AndroidBundlePrefix = "LabFusion.Core.src.Network.Riptide.resources.bundles.Android.";

        // Bundle
        public const string TideBundle = "tidebundle.fusion";

        public const string KeyboardPrefab = "ui_Keyboard";
    }
}
