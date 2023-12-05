using Static;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class NewsRect : MonoBehaviour {
        [SerializeField] private TMP_Text _titleText, _descText;
        [SerializeField] private Button _button;
        public void Init(NewsInfos info) {
            _titleText.text = info.Title;
            _descText.text = info.TagLine;

            _button.onClick.AddListener(OnClick);
        }
        public void OnClick() {
            Utils.Log("We clicked news button, {0}", _titleText.text);
        }
        private void Dispose() {
            _button.onClick.RemoveAllListeners();
        }
        public void OnDestroy() => Dispose();
        public void OnDisable() => Dispose();
    }
}