using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class MainScreenController : MonoBehaviour, IScreen {
        [SerializeField] private Button _playButton;
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null) {
            _playButton.onClick.AddListener(OnPlay);
        }
        public void Hide() {
            _playButton.onClick.RemoveAllListeners();
        }
        private void OnPlay() {
            ViewManager.ChangeView(View.Character);
        }
    }
}
