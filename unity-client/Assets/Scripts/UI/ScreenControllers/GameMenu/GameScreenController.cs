using Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GameScreenController : MonoBehaviour, IScreen {
        [SerializeField] private Button _menuButton;
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(false);
            _menuButton.onClick.AddListener(OnMenu);

            if(data == null) {
                ViewManager.ChangeView(View.Menu);
                Utils.Error("Game | Data is null, expected int value");
                return;
            }

            var init = (GameInitData)data;
            var handler = new PacketHandler(init);
            handler.Start();
        }
        public void Hide() {
            PacketHandler.Instance?.Stop();
            _menuButton.onClick.RemoveAllListeners();
        }
        private void OnMenu() {
            ViewManager.ChangeView(View.Menu);
        }
    }
}
