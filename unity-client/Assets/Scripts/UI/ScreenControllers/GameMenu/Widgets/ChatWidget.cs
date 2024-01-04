using Game;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class ChatWidget : MonoBehaviour {
        public static ChatWidget Instance { get; private set; }
        [SerializeField] private ChatInput Input;
        [SerializeField] private ChatMessage MessagePrefab;
        [SerializeField] private Transform Container;
        [SerializeField] private VerticalLayoutGroup VLG;
        [SerializeField] private Scrollbar Scroll;
        private void Awake() {
            Instance = this;
        }
        public void ClearInput() => Input.Deselect();
        public void AddMessage(Networking.Tcp.Text textPacket) {
            //Creates new message
            var obj = Instantiate(MessagePrefab, Container);
            obj.Init(GameTime.Time, textPacket); //Message deletes after 120 seconds

            //Fixes chat messages stacking onto each other
            Canvas.ForceUpdateCanvases();
            VLG.enabled = false;
            VLG.enabled = true;

            //Sets container to show latest message
            Scroll.value = 0;
        }
    }
}
