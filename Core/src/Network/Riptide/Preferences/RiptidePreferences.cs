using LabFusion.Network;
using LabFusion.Preferences;
using LabFusion.Representation;
using LabFusion.Senders;
using MelonLoader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Preferences
{
    public static class RiptidePreferences
    {
        internal static MelonPreferences_Category prefCategory;

        internal static ServerSettings LocalServerSettings;

        public class ServerSettings
        {
            public IFusionPref<ushort> ServerPort;

            public static ServerSettings CreateMelonPrefs()
            {
                // Server settings
                var settings = new ServerSettings
                {
                    // General settings
                    ServerPort = new FusionPref<ushort>(prefCategory, "Server Nametags Enabled", 7777, PrefUpdateMode.SERVER_UPDATE),
                };

                return settings;
            }
        }

        internal static void OnInitializePreferences()
        {
            // Create preferences
            prefCategory = MelonPreferences.CreateCategory("BONELAB TideFusion");

            // Server settings
            LocalServerSettings = ServerSettings.CreateMelonPrefs();
        }
    }
}
