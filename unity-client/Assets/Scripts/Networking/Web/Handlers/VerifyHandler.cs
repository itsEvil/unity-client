using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Account;

namespace Networking.Web {
    public class VerifyHandler : IWebRequest {
        public WebResponse Response { get; set; }
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

            Response = await WebSender.SendWebRequest(WebConstants.ACCOUNT_VERIFY, dict);

            int attempts = 0;
            while (Response.Result != WebResult.Success && attempts < 2) {
                Response = await WebSender.SendWebRequest(WebConstants.ACCOUNT_VERIFY, dict);
                attempts++;
            }
        }
        public void OnComplete(WebResponse response) {

            var result = response.Result == WebResult.Success;
            var threadId = Thread.CurrentThread.ManagedThreadId;

            Utils.Log("VerifyHandler : {0}, {1}, {2}", response.ResultToString(), result, threadId);

            if (result)
            {
                Utils.Log("VerifyHandler : Success Login {0}", threadId);
                AccountData.OnSuccessfulLogin(_email, _password);
                Utils.Log("VerifyHandler : Enquing char list {0}", threadId);
                new CharListHandler().Enqueue();
            }
        }
    }
}
