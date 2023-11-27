using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Account;

namespace Networking.Web {
    public class VerifyHandler : IWebRequest {
        private readonly string _email, _password;
        public VerifyHandler(string email, string password) {
            _email = email;
            _password = password;
        }
        public async Task SendAsync() {
            var dict = new Dictionary<string, string>() {
                { WebConstants.Email, _email },
                { WebConstants.Password, _password },
            };

            var result = await WebSender.SendWebRequest(WebConstants.ACCOUNT_VERIFY, dict);

            OnLogInRequestComplete(result);
        }

        private void OnLogInRequestComplete(WebResponse result) {
            if (result.Result == WebResult.Success) {
                AccountData.OnSuccessfulLogin(_email, _password);
                var charListRequest = new CharListHandler();
                WebController.WorkQueue.Enqueue(new WebWork(charListRequest));
            }
        }
    }
}
