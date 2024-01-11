using LabFusion.Network;
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

        public RiptideLobby(List<KeyValuePair<string, string>> metadata)
        {
            Metadata = metadata;
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

            foreach (var pair in Metadata)
            {
                if (pair.Key == key)
                {
                    Metadata.Remove(pair);
                }
            }

            Metadata.Add(keyPair);
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
