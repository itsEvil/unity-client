using Game.Entities;
using Networking.Tcp;
using System.Collections.Generic;
using System.Text;

namespace UI {
    public class ChatCommands {
        private static readonly Dictionary<string, ChatCommand> Commands = new()
        {
            { "help", new("help", "Provides more information about a command", ".help <commandName>", 1, ExecuteHelpCommand) },
            { "commands", new("commands", "Lists all available commands", ".commands", 0, ExecuteCommandsCommand) },
            { "emojis", new("emojis", "Lists all available emojis", ".emojis", 0, ExecuteEmojisCommand) },
        };

        public static void ParseChatCommand(string message) {
            if(message.Length <= 1) {
                SendCommandList();
                return;
            }

            var words = message[1..].Split(' ');
            var inputWords = new string[words.Length - 1];
            for (var i = 1; i < words.Length; i++)
                inputWords[i - 1] = words[i];
            var command = words[0];

            if (!Commands.TryGetValue(command, out var chatCommand)) {
                SendSystemMessage("Unknown command");
                return;
            }

            if(inputWords.Length < chatCommand.MandatoryLength) {
                SendSystemMessage("Usage: '" + chatCommand.UseCase + "'");
                return;
            }

            chatCommand.Action?.Invoke(inputWords);
        }
        private static void SendCommandList() {
            StringBuilder sb = new();
            sb.Append("\n");
            sb.Append("Valid client commands are: ");
            sb.Append("<size=14px>");
            foreach(var (_, cmd) in Commands) {
                sb.Append("\n");
                sb.Append(cmd.ToString());
            }
            sb.Append("</size>");

            SendSystemMessage(sb.ToString());
        }
        private static void ExecuteHelpCommand(string[] words) {
            var command = words[0];
            
            if(!Commands.TryGetValue(command, out var chatCommand)) {
                SendSystemMessage($"Couldn't find command '{command}'");
                return;
            }

            SendSystemMessage("Usage: '" + chatCommand.UseCase + "'");
        }
        private static void ExecuteCommandsCommand(string[] words) {
            SendCommandList();
        }
        private static void ExecuteEmojisCommand(string[] words) {
            StringBuilder sb = new();
            sb.Append("\n");
            sb.Append("Valid emojis are are: ");
            sb.Append("<size=14px>");
            foreach (var (tag, index) in ChatEmojis.TagToIndex) {
                sb.Append("\n");
                sb.Append($"Tag: '{tag}' Sprite: '<sprite index={index}>'");
            }
            sb.Append("</size>");
            

            SendSystemMessage(sb.ToString());
        }
        private static void SendSystemMessage(string message) {
            ChatWidget.Instance.AddMessage(new Text("*System*", -1, -1, 0, "", message));
        }
    }
}
