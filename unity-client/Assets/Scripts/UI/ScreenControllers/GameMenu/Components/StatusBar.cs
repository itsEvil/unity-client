using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class StatusBar : MonoBehaviour {

        [SerializeField] private Image _fillImage;
        [SerializeField] private TMP_Text _valueText, _maxValueText;
        private Player _player;
        private int _current, _max = -1;
        public void Init(Player player) {
            _player = player;
        }
        /// <summary>
        /// Tick method must be called to work
        /// </summary>
        public void Tick(int current, int max) {
            if (_player == null || _player.Dead) {
                //Destroy(gameObject);
                return;
            }

            if (_current == current && _max == max)
                return;

            _current = current;
            _max = max;

            Draw(current, max);
        }
        private void Draw(int current, int max) {
            float y = _fillImage.rectTransform.sizeDelta.y;
            float x = ((float)current / (float)max) * 100;
            _fillImage.rectTransform.sizeDelta = new Vector2(x, y);

            _valueText.text = StringUtils.GetNumericString(current);
            _maxValueText.text = StringUtils.GetNumericString(max);
        }
    }
}