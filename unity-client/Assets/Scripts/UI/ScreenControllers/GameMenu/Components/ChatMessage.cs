using Game;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ChatMessage : MonoBehaviour {
        private const int TimeAliveMS = 120_000;
        [SerializeField] private Button Button;
        [SerializeField] private TMP_Text Text;
        private int ObjectId;
        private int DeathTime;
        private void OnClick() {
            Utils.Log("We clicked on chat message with ObjectId {0}", ObjectId);

            //Get Entity from Map
        }
        public void Init(int time, Networking.Tcp.Text textPacket) {
            ObjectId = textPacket.ObjectId;
            DeathTime = time + TimeAliveMS;
            StringBuilder sb = new();
            bool addName = false;
            var color = "#FFFFFF";
            char prefix = ' ';

            if(textPacket.Name.Length > 0) {
                addName = true;
                prefix = textPacket.Name[0];
                if (textPacket.Name == "*Help*") {
                    color = "#ff9f21";
                    addName = false;
                } else if (textPacket.Name == "*Error*") {
                    color = "#ff2121";
                    addName = false;
                } else if(textPacket.Name == "*System*") {
                    color = "#ff5900";
                    addName = false;
                }
            }
            //Name

            var parsedMessage = ChatEmojis.ParseForEmojis(textPacket.Message);

            if (addName) {
                if (prefix == '@') //Admin prefix
                    sb.Append(StringUtils.AddColorTag(textPacket.Name[1..], "#f7f700"));
                else
                    sb.Append(textPacket.Name);
                
                //Spacing
                sb.Append(' ');
                sb.Append('|');
                sb.Append(' ');
                sb.Append("<noparse>");
                sb.Append(parsedMessage);
                sb.Append("</noparse>");
            }
            else {
                sb.Append(StringUtils.AddColorTag(parsedMessage, color));
            }

            //Actual message

            Text.SetText(sb.ToString(), false);
        }
        public void FixedUpdate() {
            if (DeathTime < GameTime.Time)
                Destroy(gameObject);
        }
        public void OnDestroy() {
            Button.onClick.RemoveAllListeners();
        }
        public void OnBecameVisible() {
            if (ObjectId != -1)
                Button.onClick.AddListener(OnClick);
        }
        public void OnBecameInvisible() {
            Button.onClick.RemoveAllListeners();
        }
    }
}
