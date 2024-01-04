using Game;
using Game.Entities;
using Networking.Tcp;
using Static;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class GameScreenController : MonoBehaviour, IScreen {
        public static GameScreenController Instance { get; private set; }
        [SerializeField] private StatsWidget _statsWidget;
        [SerializeField] private MinimapWidget _minimapWidget;
        [SerializeField] private PortalWidget _portalWidget;
        [SerializeField] private ChatWidget _chatWidget;
        public GameObject Object { get => gameObject; }
        private void Awake() => Instance = this;
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(false);
            SetPortalVisiblity(false);

            if(data == null) {
                ViewManager.ChangeView(View.Menu);
                Utils.Error("Game | Data is null, expected int value");
                return;
            }
            
            _chatWidget.ClearInput();

            var init = (GameInitData)data;
            var handler = new PacketHandler(init);
            handler.Start();
        }
        public void OnMyPlayerConnected(Player myPlayer) {
            _statsWidget.Init(myPlayer);
            _statsWidget.gameObject.SetActive(true);
        }
        public void OnMapInfo(MapInfo info) {
            _chatWidget.AddMessage(new Networking.Tcp.Text("", -1, -1, 0, "", $"Connecting to {info.WorldName}"));
            _chatWidget.ClearInput();
            //_minimapWidget.Init();
            //_minimapWidget.gameObject.SetActive(true);
        }
        public void Hide() {
            Map.Instance.Dispose();
            SetAllOptionalWidgetVisibility(false);
            PacketHandler.Instance?.Stop();
        }

        public void SetAllOptionalWidgetVisibility(bool value) {
            _portalWidget.gameObject.SetActive(value);
        }

        private void OnMenu() {
            ViewManager.ChangeView(View.Menu);
        }
        public void AddMessage(Networking.Tcp.Text textPacket) => _chatWidget.AddMessage(textPacket);

        void Update() {
            PacketHandler.Instance.Tick();

            if (Input.GetKeyDown(KeyCode.Escape)) {
                OnMenu();
                return;
            }

            if (Map.MyPlayer == null)
                return;

            _statsWidget.Tick();
            //_minimapWidget.Tick();
        }

        private void OnApplicationQuit() {
            PacketHandler.Instance.Stop();
        }
        private void OnDisable() {
            Hide();
        }
        public void InitPortalWidget(Interactive entity) {
             _portalWidget.Init(entity);
        }
        public void SetPortalVisiblity(bool value) {
            _portalWidget.gameObject.SetActive(value);
        }
    }
}
