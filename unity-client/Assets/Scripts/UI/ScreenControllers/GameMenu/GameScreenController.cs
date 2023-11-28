using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class GameScreenController : MonoBehaviour, IScreen {
        [SerializeField] private Button _menuButton;
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null) {
            _menuButton.onClick.AddListener(OnMenu);
        }
        public void Hide() {
            _menuButton.onClick.RemoveAllListeners();
        }
        private void OnMenu() {
            ViewManager.ChangeView(View.Menu);
        }
    }
}
