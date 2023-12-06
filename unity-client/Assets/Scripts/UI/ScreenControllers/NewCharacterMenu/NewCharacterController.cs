using Account;
using Data;
using Static;
using System;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class NewCharacterController : MonoBehaviour, IScreen {
        public static NewCharacterController Instance { get; private set; }
        [SerializeField] private NewCharacterRect _newCharRectPrefab;
        [SerializeField] private Transform _container;
        [SerializeField] private Button _backButton;
        public GameObject Object { get => gameObject; }
        public void Awake() => Instance = this;
        private NewCharacterRect[] _spawned = Array.Empty<NewCharacterRect>();
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(true);
            _backButton.onClick.AddListener(OnBack);

            //Only spawn classes once
            if (_spawned.Length != 0)
                return;

            int counter = 0;
            _spawned = new NewCharacterRect[AssetLibrary.Type2PlayerDesc.Count];
            foreach(var (type, desc) in AssetLibrary.Type2PlayerDesc) {
                var obj = Instantiate(_newCharRectPrefab, _container);
                obj.Init(desc);
                _spawned[counter++] = obj;
            }
        }
        public void Hide() => _backButton.onClick.RemoveAllListeners();
        public void OnClassSelect(short classType) => ViewManager.ChangeView(View.Skin, new GameInitData(Settings.NexusId, AccountData.NextCharId, true, classType, 0));
        private void OnBack() => ViewManager.ChangeView(View.Character);
    }
}