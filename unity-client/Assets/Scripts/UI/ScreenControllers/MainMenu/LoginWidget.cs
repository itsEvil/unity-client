using Account;
using Networking.Web;
using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class LoginWidget : MonoBehaviour, IScreen {
        [SerializeField] private TMP_InputField _emailInput, _passwordInput;
        [SerializeField] private Button _loginButton, _registerButton;
        public GameObject Object => gameObject;
        private MainScreenController Parent;
        public void Reset(object data = null) {
            if (data == null)
                throw new Exception("Data is null");

            Parent = (MainScreenController)data;
            SetButtonFunctionality(true);
            _loginButton.onClick.AddListener(OnLogin);
            _registerButton.onClick.AddListener(OnRegister);
        }
        public void Hide() {
            _loginButton.onClick.RemoveAllListeners();
            _registerButton.onClick.RemoveAllListeners();
        }
        /// <summary>
        /// Switches to Register Widget
        /// </summary>
        private void OnRegister() {
            Parent.ToggleWidgets();
        }
        /// <summary>
        /// Sends Login request
        /// </summary>
        private void OnLogin() {
            Parent.OnError("");

            SetButtonFunctionality(false);
            using var d = new DeferAction(() => { SetButtonFunctionality(true); });

            var email = _emailInput.text;
            var pass = _passwordInput.text;

            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass)) {
                Parent.OnError("LoginValuesAreNullOrEmpty");
                return;
            }

            d.Cancel = true;
            Requests.TryLogin(email, pass);
        }
        public void SetButtonFunctionality(bool val) {
            _loginButton.interactable = val;
            _registerButton.interactable = val;
        }
    }
}