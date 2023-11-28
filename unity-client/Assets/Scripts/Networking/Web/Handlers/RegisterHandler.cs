using Account;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Networking.Web {
    public class RegisterHandler : IWebRequest {
        private readonly string _newEmail;
        private readonly string _newPassword;
        private readonly string _newPlayerName;
        public RegisterHandler(string email, string password, string playerName) {
            _newEmail = email;
            _newPlayerName = playerName;
            _newPassword = password;
        }
        public void Enqueue() {
            WebController.WorkQueue.Enqueue(new WebWork(this));
        }
        public async Task SendAsync() {
            var dict = new Dictionary<string, string>() {
                { WebConstants.NewEmail, _newEmail },
                { WebConstants.NewPassword, _newPassword },
                { WebConstants.NewUsername, _newPlayerName },
            };

            var result = await WebSender.SendWebRequest(WebConstants.ACCOUNT_REGISTER, dict);

            OnRegisterRequestComplete(result);
        }

        private void OnRegisterRequestComplete(WebResponse result) {
            if (result.Result == WebResult.Success) {
                AccountData.OnSuccessfulLogin(_newEmail, _newPassword);
                new CharListHandler().Enqueue();
            }
            else {
                Requests.OnLoginResult?.Invoke(false);
            }
        }
    }
}
