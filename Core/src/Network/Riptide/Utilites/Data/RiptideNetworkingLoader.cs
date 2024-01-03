using LabFusion.Data;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Utilities
{
    internal class RiptideNetworkingLoader
    {
        internal static void Load()
        {
            string sdkPath = PersistentData.GetPath($"RiptideNetworking.dll");
            File.WriteAllBytes(sdkPath, EmbeddedResource.LoadFromAssembly(FusionMod.FusionAssembly, LabFusion.Riptide.Utilities.ResourcePaths.RiptideNetworkingPath));

            MelonLoader.MelonAssembly.LoadMelonAssembly(sdkPath);
        }
    }
}
