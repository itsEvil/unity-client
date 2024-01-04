using System.Collections.Generic;
using System.Text;

namespace UI {
    public class ChatEmojis {
        public static Dictionary<string, string> TagToIndex = new() {
            { ":smile:", "14" },
            { ":sad:", "15" },
            { ":heart_eyes:", "2" },
            { ":lmao:", "6" },
            { ":rofl:", "13" },
            { ":sweat_smile:", "9" },
            { ":grin:", "8" },
            { ":?:", "12" },
            { ":wink:", "11" },
            { ":sunglasses:", "3" },
        };
        /// <summary>
        /// Takes input string and returns a parsed version if it contains any emojis or the input string.
        /// </summary>
        public static string ParseForEmojis(string message) {
            var words = message.Split(' ');
            StringBuilder sb = new(message);
            bool returnNormal = true;
            bool onlyOne = false;
            if (words.Length == 1)
                onlyOne = true;

            foreach(var word in words) {
                if (TagToIndex.TryGetValue(word, out var index)) {
                    returnNormal = false;
                    
                    if(!onlyOne)
                        sb.Replace(word, $"</noparse><sprite index={index}><noparse>");
                    else 
                        sb.Replace(word, $"</noparse><size=32><sprite index={index}></size><noparse>");
                }
            }

            if (returnNormal) {
                return message;
            }
            else return sb.ToString();
        }
    }
}
