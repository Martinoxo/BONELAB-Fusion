using LabFusion.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riptide;
using MelonLoader;
using LabFusion.Utilities;
using LabFusion.Riptide.Preferences;

namespace LabFusion.Riptide
{
    // Yes, Im stealing a lot of ProxyLobbyManager code for this, but I dont care, cause I didnt want to do this in the first place.
    public class RiptideLobbyManager
    {
        private TaskCompletionSource<RiptideLobby[]> _lobbySource = null;
        private readonly ProxyNetworkLayer _networkLayer;
        private readonly Dictionary<ulong, TaskCompletionSource<LobbyMetadataInfo>> _metadataInfoRequests = new();

        private List<RiptideLobby> _lobbies = new();
        internal void HandleRiptideMessage(Message message)
        {
            if (_lobbySource == null)
            {
                return;
            }

            int lobbyCount = message.GetInt();
#if DEBUG
            FusionLogger.Log($"Got {lobbyCount} lobbies");
#endif
            if (lobbyCount == 0)
            {
                _lobbySource.SetResult(new RiptideLobby[0]);
                _lobbySource = null;
                _lobbies = new();
                return;
            }

            ushort hostId = message.GetUShort();
            int metadataCount = message.GetInt();

            List<KeyValuePair<string, string>> metadata = new List<KeyValuePair<string, string>>();

            for (int j = 0; j < metadataCount; j++)
            {
                string key = message.GetString();
                string value = message.GetString();

                metadata.Add(new KeyValuePair<string, string>(key, value));
            }

            RiptideLobby lobby = new(metadata, hostId);
#if DEBUG
            FusionLogger.Log($"Got Lobby: {hostId}");
#endif
            _lobbies.Add(lobby);

            // Finish the task
            if (_lobbies.Count == lobbyCount)
            {
                _lobbySource.SetResult(_lobbies.ToArray());
                _lobbySource = null;
                _lobbies = new();
            }
        }

        public Task<RiptideLobby[]> RequestLobbies()
        {
            _lobbySource = new TaskCompletionSource<RiptideLobby[]>();

            if (ClientManagement.PublicLobbyClient.IsConnected)
                ClientManagement.PublicLobbyClient.Send(Messages.PublicLobbyMessages.CreateRequestLobbiesMessage());
            else
            {
                ClientManagement.PublicLobbyClient.Connected += (x, y) =>
                {
                    ClientManagement.PublicLobbyClient.Send(Messages.PublicLobbyMessages.CreateRequestLobbiesMessage());
                };
                ClientManagement.PublicLobbyClient.Connect($"{RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.GetValue()}:6666", 5, 0, Messages.PublicLobbyMessages.CreateRequestLobbiesMessage(), false);
            }

            return _lobbySource.Task;
        }
    }
}
