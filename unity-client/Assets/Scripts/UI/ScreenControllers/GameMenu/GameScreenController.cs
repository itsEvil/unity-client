using Game;
using Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GameScreenController : MonoBehaviour, IScreen {
        public static GameScreenController Instance { get; private set; }
        [SerializeField] private Button _menuButton;
        [SerializeField] private StatsWidget _statsWidget;
        [SerializeField] private MinimapWidget _minimapWidget;
        public GameObject Object { get => gameObject; }
        private void Awake() => Instance = this;
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
        public void OnMyPlayerConnected(Player myPlayer) {
            _minimapWidget.Init();
            _statsWidget.Init(myPlayer);
            _minimapWidget.gameObject.SetActive(true);
            _statsWidget.gameObject.SetActive(true);
        }
        public void Hide() {
            PacketHandler.Instance?.Stop();
            _menuButton.onClick.RemoveAllListeners();
            _minimapWidget.gameObject.SetActive(false);
            _statsWidget.gameObject.SetActive(false);
        }
        private void OnMenu() {
            ViewManager.ChangeView(View.Menu);
        }

        void Update() {
            if (Map.MyPlayer == null)
                return;

            _statsWidget.Tick();
            _minimapWidget.Tick();
        }
    }
}
