using Account;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UI;

namespace Networking.Web {
    public class RegisterHandler : IWebRequest {
        public WebResponse Response { get; set; }
        private readonly string _email;
        private readonly string _pass;
        private readonly string _playerName;
        public RegisterHandler(string email, string password, string playerName) {
            _email = email;
            _playerName = playerName;
            _pass = password;
        }
        public async Task SendAsync() {
            var dict = new Dictionary<string, string>() {
                { WebConstants.Email, _email },
                { WebConstants.Password, _pass },
                { WebConstants.Username, _playerName },
            };

            Response = await WebSender.SendWebRequest(WebConstants.ACCOUNT_REGISTER, dict);

            int attempts = 0;
            while (Response.Result != WebResult.Success && attempts < 2) {
                Response = await WebSender.SendWebRequest(WebConstants.ACCOUNT_REGISTER, dict);
                attempts++;
            }
        }
        public void OnComplete(WebResponse response) {
            if (response.Result == WebResult.Success) {
                AccountData.OnSuccessfulLogin(_email, _pass);
                new CharListHandler().Enqueue();
                return;
            }

            MainScreenController.Instance.OnLogin(false);
        }
    }
}
