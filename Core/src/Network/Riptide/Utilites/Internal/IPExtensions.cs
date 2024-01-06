using Harmony;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide.Utilities
{
    internal static class IPExtensions
    {
        public static uint EncodeIpAddress(string ipAddress)
        {
            var segments = ipAddress.Split('.');
            if (segments.Length != 4)
                throw new FormatException("Invalid IP address format.");

            uint encoded = 0;
            for (int i = 0; i < 4; i++)
            {
                uint segment = uint.Parse(segments[i]);
                if (segment > 255)
                    throw new FormatException("Invalid IP address format.");

                encoded = (encoded << 8) | segment;
            }

            return encoded;
        }

        public static string DecodeIpAddress(uint encodedIpAddress)
        {
            var segments = new string[4];
            for (int i = 0; i < 4; i++)
            {
                segments[3 - i] = ((encodedIpAddress >> (i * 8)) & 255).ToString();
            }

            return string.Join(".", segments);
        }

    }
}
