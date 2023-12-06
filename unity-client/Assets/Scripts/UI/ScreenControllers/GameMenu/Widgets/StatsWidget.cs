using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class StatsWidget : MonoBehaviour {
        [SerializeField] private Button _extraStatsButton;
        [SerializeField] private TMP_Text _attText, _defText, _spdText, _dexText, _wisText, _vitText, _levelText;
        [SerializeField] private StatusBar _xpBar, _fameBar, _hpBar, _mpBar;
        [SerializeField] private GameObject _extraContainer;
        private Player _player;
        public void Init(Player player) {
            _extraContainer.SetActive(false);
            _extraStatsButton.onClick.AddListener(OnExtra);
            _player = player;

            UpdateAllText();
        }
        public void UpdateAllText() {
            if (_player.Level < Player.MaxLevel && !_xpBar.gameObject.activeSelf) {
                _xpBar.gameObject.SetActive(true);
                _fameBar.gameObject.SetActive(false);
            } else if(_player.Level == Player.MaxLevel && !_fameBar.gameObject.activeSelf) {
                _xpBar.gameObject.SetActive(false);
                _fameBar.gameObject.SetActive(true);
            }
            
            UpdateLevelText();
            UpdateAttackText();
            UpdateDefenseText();
            UpdateDexterityText();
            UpdateSpeedText();
            UpdateVitalityText();
            UpdateWisdomText();
        }
        public void UpdateLevelText() 
            => _levelText.text = StringUtils.GetNumericString(_player.Level);
        public void UpdateAttackText()
            => _attText.text = StringUtils.GetNumericString(_player.Attack);
        public void UpdateDefenseText()
            => _dexText.text = StringUtils.GetNumericString(_player.Defense);
        public void UpdateSpeedText()
            => _spdText.text = StringUtils.GetNumericString(_player.Speed);
        public void UpdateDexterityText()
            => _dexText.text = StringUtils.GetNumericString(_player.Dexterity);
        public void UpdateVitalityText()
            => _vitText.text = StringUtils.GetNumericString(_player.Vitality);
        public void UpdateWisdomText()
            => _wisText.text = StringUtils.GetNumericString(_player.Wisdom);
        public void Tick() {
            _hpBar.Tick(_player.Hp, _player.MaxHp);
            _mpBar.Tick(_player.Mp, _player.MaxMp);

            if (_player.Level < Player.MaxLevel)
                _xpBar.Tick(_player.CurrentExp, _player.NextLevelExp);
            else _fameBar.Tick(_player.CurrentFame, _player.NextFameGoal);
        }
        private void OnExtra() 
            => _extraContainer.SetActive(!_extraContainer.activeSelf);
        private void OnDisable() {
            _extraStatsButton.onClick.RemoveAllListeners();
        }
    }
}