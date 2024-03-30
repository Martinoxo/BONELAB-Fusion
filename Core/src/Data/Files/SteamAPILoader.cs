using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using LabFusion.Utilities;

using BoneLib;

namespace LabFusion.Data
{
    public static class SteamAPILoader
    {
        public static bool HasSteamAPI { get; private set; } = false;

        private static IntPtr _libraryPtr;

        public static void OnLoadSteamAPI()
        {
            // If it's already loaded, don't load it again
            if (HasSteamAPI)
                return;
        }

        public static void OnFreeSteamAPI()
        {
            // Don't unload it if it isn't loaded
            if (!HasSteamAPI)
                return;

            DllTools.FreeLibrary(_libraryPtr);

            HasSteamAPI = false;
        }
    }
}
