using Account;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Linq;
using UI;

namespace Networking.Web {
    public class CharListHandler : IWebRequest {
        public void Enqueue() {
            WebController.WorkQueue.Enqueue(new WebWork(this));
        }
        public async Task SendAsync() {
            var dict = new Dictionary<string, string>() {
                    { WebConstants.Email, AccountData.GetEmail() },
                    { WebConstants.Password, AccountData.GetPassword() }
                };

            var result = await WebSender.SendWebRequest(WebConstants.CHAR_LIST, dict);

            int attempts = 0;
            while (result.Result != WebResult.Success && attempts < 2) {
                result = await WebSender.SendWebRequest(WebConstants.CHAR_LIST, dict);
                attempts++;
            }

            OnListRequestComplete(result);
        }

        private void OnListRequestComplete(WebResponse response) {
            Utils.Log("CharListResponse:{0}", response.Reply);
            if (response.Result == WebResult.Success) {
                AccountData.LoadFromCharList(XElement.Parse(response.Reply));
            }

            Requests.OnLoginResult?.Invoke(response.Result == WebResult.Success);
        }
    }
}
