using Account;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Networking.Web {
    public class RegisterHandler : IWebRequest {
        private readonly string _email;
        private readonly string _pass;
        private readonly string _playerName;
        public RegisterHandler(string email, string password, string playerName) {
            _email = email;
            _playerName = playerName;
            _pass = password;
        }
        public void Enqueue() {
            WebController.WorkQueue.Enqueue(new WebWork(this));
        }
        public async Task SendAsync() {
            var dict = new Dictionary<string, string>() {
                { WebConstants.Email, _email },
                { WebConstants.Password, _pass },
                { WebConstants.Username, _playerName },
            };

            var result = await WebSender.SendWebRequest(WebConstants.ACCOUNT_REGISTER, dict);

            OnRegisterRequestComplete(result);
        }

        private void OnRegisterRequestComplete(WebResponse result) {
            if (result.Result == WebResult.Success) {
                AccountData.OnSuccessfulLogin(_email, _pass);
                new CharListHandler().Enqueue();
            }
            else {
                Requests.OnLoginResult?.Invoke(false);
            }
        }
    }
}
