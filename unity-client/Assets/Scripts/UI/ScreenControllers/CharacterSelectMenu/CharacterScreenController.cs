using Account;
using Static;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
namespace UI {
    public class CharacterScreenController : MonoBehaviour, IScreen {
        public static CharacterScreenController Instance { get; private set; }
        [SerializeField] private NewsRect _newsPrefab;
        [SerializeField] private CharacterRect _charRectPrefab;
        [SerializeField] private Transform _charContainer, _newsContainer;
        [SerializeField] private Button _playButton, _backButton;
        private CharacterRect[] _spawned = Array.Empty<CharacterRect>();
        private NewsRect[] _news = Array.Empty<NewsRect>();
        private int _firstCharId;
        public GameObject Object { get => gameObject; }
        private void Awake() => Instance = this;
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(true);
            _firstCharId = -1;
            _playButton.onClick.AddListener(OnPlay);
            _backButton.onClick.AddListener(OnBack);

            if(_news.Length != 0) {
                foreach (var obj in _news)
                    Destroy(obj.gameObject);
            }

            if(_spawned.Length != 0) {
                foreach(var obj in _spawned) 
                    Destroy(obj.gameObject);
            }

            int totalChars = AccountData.MaxCharacters;
            _spawned = new CharacterRect[totalChars];

            if(AccountData.Characters.Count > 0) {
                _firstCharId = AccountData.Characters[0].Id;
            }
            //Characters
            for(int i = 0; i < AccountData.Characters.Count; i++) {
                var obj = Instantiate(_charRectPrefab, _charContainer);
                obj.Init(AccountData.Characters[i]);
                _spawned[i] = obj;
            }

            //Empty slots
            for(int i = AccountData.Characters.Count; i < totalChars; i++) {
                var obj = Instantiate(_charRectPrefab, _charContainer);
                obj.Init();
                _spawned[i] = obj;
            }

            _news = new NewsRect[AccountData.News.Count];
            for(int i = 0; i < _news.Length; i++) {
                var obj = Instantiate(_newsPrefab, _newsContainer);
                obj.Init(AccountData.News[i]);
                _news[i] = obj;
            }
        }
        private void OnBack()
            => ViewManager.ChangeView(View.Menu);
        public void Hide() {
            _playButton.onClick.RemoveAllListeners();
            _backButton.onClick.RemoveAllListeners();
        }
        private void OnPlay() 
            => SwitchToPlay(_firstCharId);
        public void SwitchToPlay(int id) {
            if (id == -1) {
                ViewManager.ChangeView(View.NewCharacter);
                return;
            }
            ViewManager.ChangeView(View.Game, new GameInitData(Settings.NexusId, (short)id, false, 0, 0));
        }
    }
}
