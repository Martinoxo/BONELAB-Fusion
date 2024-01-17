using LobbyHost.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobbyHost.UI
{
    internal class Commands
    {
        internal static void HandleCommand(string input)
        {
            string[] args = input.Split(' ');
            string command = args[0];
            switch (command)
            {
                case "close":
                    {
                        Environment.Exit(0);
                        break;
                    }
                case "help":
                    {
                        TUIManager.RefreshUi(
                        "close - Closes the server\n" +
                        "help - Displays this message\n" +
                        "lobbyinfo [hostId] - Displays info about a specific lobby");
                        break;
                    }
                case "lobbyinfo":
                    {
                        if (args.Length != 2)
                        {
                            TUIManager.RefreshUi("Invalid command, type \"help\" for a list of commands.");
                            break;
                        }

                        if (!ushort.TryParse(args[1], out ushort hostId))
                        {
                            TUIManager.RefreshUi("Invalid host ID.");
                            break;
                        }

                        var lobby = Lobby.GetHostLobby(hostId);
                        if (lobby == null)
                        {
                            TUIManager.RefreshUi($"Lobby with Host ID {hostId} doesn't exist.");
                            break;
                        }

                        string info =
                            $"Host ID: {lobby.HostId}\n" +
                            $"Client Count: {lobby.Clients.Count}\n";
                        
                        TUIManager.RefreshUi(info);
                        break;
                    }
                default:
                    {
                        TUIManager.RefreshUi("Invalid command, type \"help\" for a list of commands.");
                        break;
                    }
            }
        }
    }
}
