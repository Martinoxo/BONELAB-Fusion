using BoneLib.BoneMenu.Elements;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnhollowerBaseLib;
using Il2CppNewtonsoft.Json;
using System.Web;

namespace LabFusion.Riptide.Utilities
{
    internal static class ServerListSaving
    {
        private static readonly string ServerListPath = $"{RiptideNetworkLayer.TideFusionPath}/ServerListings";
        internal static void SaveServerList(ServerListData data)
        {
            if (!System.IO.Directory.Exists(ServerListPath))
                System.IO.Directory.CreateDirectory(ServerListPath);

            int count = 1;
            foreach (var file in System.IO.Directory.GetFiles(ServerListPath))
            {
                count++;
            }

#if DEBUG
            FusionLogger.Log($"Saving data {count}:\n" +
                $"{data.Name}\n" +
                $"{data.ServerCode}\n" +
                $"{data.Port}");
#endif

            var text = System.IO.File.CreateText(ServerListPath + $"/ServerListing {count}.txt");
            text.WriteLine(data.Name);
            text.WriteLine(data.ServerCode);
            text.WriteLine(data.Port);
            text.Close();
        }

        internal static ServerListData[] LoadServerList()
        {
            if (!System.IO.Directory.Exists(ServerListPath))
                System.IO.Directory.CreateDirectory(ServerListPath);

            List<ServerListData> dataList = new List<ServerListData>();

            foreach (var filePath in System.IO.Directory.GetFiles(ServerListPath))
            {
                string[] fileData = System.IO.File.ReadAllLines(filePath);
                FusionLogger.Log($"Reading data from file {System.IO.Path.GetFileName(filePath)}");
                ServerListData data = new ServerListData();
                data.Name = fileData[0];
                data.ServerCode = fileData[1];
                data.Port = int.Parse(fileData[2]);
                data.ServerListDataPath = filePath;

                dataList.Add(data);
            }

            return dataList.ToArray();
        }
    }

    public class ServerListData
    {
        public string Name;
        public string ServerCode;
        public int Port = 7777;
        public string ServerListDataPath = string.Empty;
        //TODO: SERVER KEYS
    }
}
