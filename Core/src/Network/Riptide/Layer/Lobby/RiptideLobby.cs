using LabFusion.Network;
using LabFusion.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabFusion.Riptide
{
    public class RiptideLobby : NetworkLobby
    {
        public List<KeyValuePair<string, string>> Metadata = new List<KeyValuePair<string, string>>();
        public ushort HostId;

        public RiptideLobby(List<KeyValuePair<string, string>> metadata, ushort hostId)
        {
            Metadata = metadata;
            HostId = hostId;
        }

        public override Action CreateJoinDelegate(ulong lobbyId)
        {
            return () => ClientManagement.JoinPublicLobby((ushort)lobbyId);
        }

        public override string GetMetadata(string key)
        {
            foreach (var pair in Metadata)
            {
                if (pair.Key == key)
                    return pair.Value;
            }

            return string.Empty;
        }

        public override void SetMetadata(string key, string value)
        {
            KeyValuePair<string, string> keyPair = new KeyValuePair<string, string>(key, value);

            Metadata.RemoveAll(pair => pair.Key == key);

            Metadata.Add(keyPair);

            if (ClientManagement.PublicLobbyClient.IsConnected)
                ClientManagement.PublicLobbyClient.Send(Messages.PublicLobbyMessage.CreateUpdateLobbyMessage(key, value));

            SaveKey(key);
        }

        public override bool TryGetMetadata(string key, out string value)
        {
            foreach (var pair in Metadata)
            {
                if (pair.Key == key)
                {
                    value = pair.Value;
                    return true;
                }
            }

            value = string.Empty;
            return false;
        }
    }
}
