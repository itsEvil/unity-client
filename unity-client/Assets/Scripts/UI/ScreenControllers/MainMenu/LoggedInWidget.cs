using Account;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class LoggedInWidget : MonoBehaviour, IScreen {
        [SerializeField] private TMP_Text _loggedInText;
        [SerializeField] private Button _logOutButton;
        public GameObject Object => gameObject;
        private MainScreenController Parent;
        public void Reset(object data = null) {
            if (data == null)
                throw new Exception("Data is null");

            Parent = (MainScreenController)data;
            _logOutButton.onClick.AddListener(OnLogOut);
        }
        public void OnLoggedIn(string name) {
            _loggedInText.text = $"Logged in as {name}";
        }
        public void Hide() {
            _logOutButton.onClick.RemoveAllListeners();
        }
        private void OnLogOut() {
            AccountData.Reset();
            Parent.OnLogout();
        }
    }
}