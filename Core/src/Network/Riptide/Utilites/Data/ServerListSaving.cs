﻿using LabFusion.Utilities;
using System.Collections.Generic;
using System.IO;

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
                count++;

            string path = ServerListPath + $"/ServerListing_{count}.json";

            var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            File.WriteAllText(path, jsonData);
        }

        internal static ServerListData[] LoadServerList()
        {
            if (!System.IO.Directory.Exists(ServerListPath))
                System.IO.Directory.CreateDirectory(ServerListPath);

            List<ServerListData> dataList = new List<ServerListData>();

            foreach (var filePath in System.IO.Directory.GetFiles(ServerListPath))
            {
                if (!Path.GetFileName(filePath).StartsWith("ServerListing_"))
                {
                    // Delete the file if it's in the old format (before json)
                    System.IO.File.Delete(filePath);
                    continue;
                }

                string jsonText = System.IO.File.ReadAllText(filePath);
                ServerListData data = Newtonsoft.Json.JsonConvert.DeserializeObject<ServerListData>(jsonText);
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
        public ushort Port = 7777;
        public string ServerListDataPath = string.Empty;
        //TODO: SERVER KEYS
    }
}
