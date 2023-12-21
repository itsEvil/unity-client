using Game;
using Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GameScreenController : MonoBehaviour, IScreen {
        public static GameScreenController Instance { get; private set; }
        public static bool DisablePlayerInput = false;
        [SerializeField] private StatsWidget _statsWidget;
        [SerializeField] private MinimapWidget _minimapWidget;
        public GameObject Object { get => gameObject; }
        private void Awake() => Instance = this;
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(false);

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
            _minimapWidget.gameObject.SetActive(false);
            _statsWidget.gameObject.SetActive(false);
        }
        private void OnMenu() {
            ViewManager.ChangeView(View.Menu);
        }

        void Update() {
            PacketHandler.Instance.Tick();

            if (Input.GetKeyDown(KeyCode.Escape)) {
                OnMenu();
                return;
            }

            if (Map.MyPlayer == null)
                return;

            _statsWidget.Tick();
            _minimapWidget.Tick();
        }

        private void OnApplicationQuit() {
            PacketHandler.Instance.Stop();
        }
        private void OnDisable() {
            Hide();
        }
    }
}
