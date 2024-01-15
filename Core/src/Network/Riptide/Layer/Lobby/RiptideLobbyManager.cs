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

        internal void HandleRiptideMessage(Message message)
        {
            if (_lobbySource == null)
            {
                FusionLogger.Error("Got extra RequestLobbies response?");
                return;
            }

            int lobbyCount = message.GetInt();
            FusionLogger.Log($"Got {lobbyCount} lobbies");

            RiptideLobby[] lobbies = new RiptideLobby[lobbyCount];

            for (int i = 0; i < lobbyCount; i++)
            {
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
                FusionLogger.Log($"Got Lobby: {hostId}");

                lobbies[i] = lobby;
            }

            // Finish the task
            _lobbySource.SetResult(lobbies);
            _lobbySource = null;
        }

        public Task<RiptideLobby[]> RequestLobbies()
        {
            _lobbySource = new TaskCompletionSource<RiptideLobby[]>();

            if (ClientManagement.PublicLobbyClient.IsConnected)
                ClientManagement.PublicLobbyClient.Send(Messages.PublicLobbyMessage.CreateRequestLobbiesMessage());
            else
            {
                ClientManagement.PublicLobbyClient.Connected += (x, y) =>
                {
                    ClientManagement.PublicLobbyClient.Send(Messages.PublicLobbyMessage.CreateRequestLobbiesMessage());
                };
                ClientManagement.PublicLobbyClient.Connect($"{RiptidePreferences.LocalServerSettings.PublicLobbyServerIp.GetValue()}:6666", 5, 0, Messages.PublicLobbyMessage.CreateRequestLobbiesMessage(), false);
            }

            return _lobbySource.Task;
        }
    }
}
