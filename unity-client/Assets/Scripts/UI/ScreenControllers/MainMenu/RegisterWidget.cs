using Account;
using Networking.Web;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI {
    public class RegisterWidget : MonoBehaviour, IScreen {
        [SerializeField] private TMP_InputField _emailInput, _nameInput, _passwordInput, _passwordInputRepeat;
        [SerializeField] private Button _loginButton, _registerButton;
        public GameObject Object => gameObject;
        private MainScreenController Parent;
        public void Reset(object data = null) {
            if (data == null)
                throw new Exception("Data is null");

            Parent = (MainScreenController)data;
            _loginButton.onClick.AddListener(OnLogin);
            _registerButton.onClick.AddListener(OnRegister);
        }
        public void Hide() {
            _loginButton.onClick.RemoveAllListeners();
            _registerButton.onClick.RemoveAllListeners();
        }
        /// <summary>
        /// Sends register request 
        /// </summary>
        private void OnRegister() {
            Parent.OnError("");

            SetButtonFunctionality(false);
            using var d = new DeferAction(() => { SetButtonFunctionality(true); });

            var email = _emailInput.text;
            var pass = _passwordInput.text;
            var passRepeat = _passwordInputRepeat.text;
            var name = _nameInput.text;

            if(string.IsNullOrEmpty(email) || string.IsNullOrEmpty(pass) || string.IsNullOrEmpty(passRepeat) || string.IsNullOrEmpty(name)) {
                Parent.OnError("RegisterValuesAreNullOrEmpty");
                return;
            }

            if(pass != passRepeat) {
                Parent.OnError("PasswordsDontMatch");
                return;
            }

            if(pass.Length < 8) {
                Parent.OnError("PasswordTooShort");
                return;
            }

            d.Cancel = true;
            Requests.TryRegister(email, pass, name);
        }
        /// <summary>
        /// Switches back to Login Widget
        /// </summary>
        private void OnLogin() {
            Parent.ToggleWidgets();
        }

        public void SetButtonFunctionality(bool val) {
            //_loginButton.interactable = val;
            //_registerButton.interactable = val;
        }
    }
}