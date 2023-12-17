using Account;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using UI;

namespace Networking.Web {
    public class CharListHandler : IWebRequest {
        public WebResponse Response { get; set; }
        public async Task SendAsync() {
            var dict = new Dictionary<string, string>() {
                    { WebConstants.Email, AccountData.GetEmail() },
                    { WebConstants.Password, AccountData.GetPassword() }
                };

            Response = await WebSender.SendWebRequest(WebConstants.CHAR_LIST, dict);

            int attempts = 0;
            while (Response.Result != WebResult.Success && attempts < 2) {
                Response = await WebSender.SendWebRequest(WebConstants.CHAR_LIST, dict);
                attempts++;
            }
        }
        public void OnComplete(WebResponse response) {
            Utils.Log("CharListResponse:{0}", response.Reply);

            var val = response.Result == WebResult.Success;

            if (val)
            {
                AccountData.LoadFromCharList(XElement.Parse(response.Reply));
            }

            MainScreenController.Instance.OnLogin(val);
        }
    }
}
