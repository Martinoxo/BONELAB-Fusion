using LobbyHost.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LobbyHost.Utilities;

namespace LobbyHost.UI
{
    internal class TUIManager
    {
        internal static void RefreshUi(string info = "")
        {
            Console.Clear();

            Console.WriteLine($"Info:\n" +
                $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" +
                $"Current Lobby Count: {ServerInfo.LobbyCount}\n" +
                $"Current Player Count: {ServerInfo.PlayerCount}\n" +
                $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

            if (info == null || info == string.Empty)
                info = "No info to display";

            Console.WriteLine($"Output:\n" +
                $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n" +
                $"{info}\n" +
                $"~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\n");

            Console.WriteLine("Type \"close\" to exit, type \"help\" for a list of commands.");
        }

        internal static void RefreshCommandUi()
        {
            string input = Console.ReadLine();

            Commands.HandleCommand(input);

            RefreshCommandUi();
        }
    }
}
