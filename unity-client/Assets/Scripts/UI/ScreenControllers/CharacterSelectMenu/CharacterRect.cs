using Data;
using Static;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class CharacterRect : MonoBehaviour {
        [SerializeField] private TMP_Text _nameText, _maxedText;
        [SerializeField] private Image _charImage;
        [SerializeField] private Button _button;
        private int _id;        
        public void Init(CharacterStats stats) {

            var desc = AssetLibrary.GetPlayerDesc((ushort)stats.ClassType);
            Utils.Log("Created character rect {0} : {1}", desc.DisplayId, desc.Type);
            _id = stats.Id;
            _charImage.sprite = desc.TextureData.GetTexture(0);
            _nameText.text = desc.DisplayId;

            _maxedText.text = desc.GetMaxedStats(stats.Stats) + " / " + desc.Stats.Length;
        }
        public void Init() {
            _id = -1;
            _maxedText.text = string.Empty;
            _nameText.text = "Empty";
            _charImage.sprite = null;
        }
        public void OnDestroy()
            => _button.onClick.RemoveAllListeners();
        public void OnDisable()
            => _button.onClick.RemoveAllListeners();
        public void OnEnable() 
            => _button.onClick.AddListener(OnClick);
        private void OnClick() 
            => CharacterScreenController.Instance.SwitchToPlay(_id);
    }
}