using Account;
using Networking.Web;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace UI {
    public class MainScreenController : MonoBehaviour, IScreen {
        [SerializeField] private TMP_Text _errorText;
        [SerializeField] private Button _playButton;
        [SerializeField] private LoginWidget _loginWidget;
        [SerializeField] private RegisterWidget _registerWidget;
        [SerializeField] private LoggedInWidget _loggedInWidget;
        public GameObject Object { get => gameObject; }
        public void Reset(object data = null) {
            ViewManager.SetBackgroundVisiblity(true);
            Requests.OnLoginResult += OnLogin;

            _playButton.onClick.AddListener(OnPlay);

            _loginWidget.Reset(this);
            _registerWidget.Reset(this);
            _loggedInWidget.Reset(this);
            _errorText.gameObject.SetActive(false);
            _loginWidget.gameObject.SetActive(true);
            _playButton.interactable = false;
            AccountData.TryLoginWithSavedData();
        }
        public void OnLogin(bool value) {

            Utils.Log("GotLogin, {0}", value);

            _playButton.interactable = true;
            _loginWidget.SetButtonFunctionality(true);
            _registerWidget.SetButtonFunctionality(true);
            _errorText.gameObject.SetActive(false);

            if (value) {
                _loggedInWidget.OnLoggedIn(AccountData.PlayerName);
                _loggedInWidget.gameObject.SetActive(value);
                _loginWidget.gameObject.SetActive(false);
                _registerWidget.gameObject.SetActive(false);
            }
        }
        public void OnLogout() {
            _errorText.gameObject.SetActive(false);
            _playButton.interactable = false;
            _loginWidget.SetButtonFunctionality(true);
            _loggedInWidget.gameObject.SetActive(false);
            _loginWidget.gameObject.SetActive(true);
        }
        public void ToggleWidgets() {
            _playButton.interactable = false;
            _loginWidget.gameObject.SetActive(!_loginWidget.gameObject.activeSelf);
            _registerWidget.gameObject.SetActive(!_registerWidget.gameObject.activeSelf);
        }
        public void Hide() {
            _errorText.gameObject.SetActive(false);
            _playButton.interactable = false;
            Requests.OnLoginResult -= OnLogin;

            _playButton.onClick.RemoveAllListeners();
            _loginWidget.Hide();
            _registerWidget.Hide();
            _loggedInWidget.Hide();
        }
        public void OnError(string txt) {
            _errorText.gameObject.SetActive(true);
            _errorText.text = txt;
        }
        private void OnPlay() {
            ViewManager.ChangeView(View.Character);
        }
    }
}
