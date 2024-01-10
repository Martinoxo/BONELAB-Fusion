using LabFusion.Preferences;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Preferences
{
    /// <summary>
    /// Class for creating FusionPreferences in a separate MelonPreferences category.
    /// </summary>
    public static class RiptidePreferences
    {
        internal static MelonPreferences_Category TideCategory = MelonPreferences.CreateCategory("BONELAB TideFusion");

        public class ServerSettings
        {
            public IFusionPref<ushort> ServerPort;
            public IFusionPref<string> PublicLobbyServerIp;

            public static ServerSettings CreateMelonPrefs()
            {
                return new ServerSettings()
                {
                    ServerPort = new FusionPref<ushort>(TideCategory, "ServerPort", 7777),
                    PublicLobbyServerIp = new FusionPref<string>(TideCategory, "PublicLobbyServerIp", "172.0.0.1")
                };
            }
        }

        internal static ServerSettings LocalServerSettings;

        internal static void OnInitializePreferences()
        {
            TideCategory = MelonPreferences.CreateCategory("BONELAB TideFusion");

            // Server settings
            LocalServerSettings = ServerSettings.CreateMelonPrefs();
        }
    }
}
