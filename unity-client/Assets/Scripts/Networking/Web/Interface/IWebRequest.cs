using System.Threading.Tasks;

namespace Networking.Web {
    public interface IWebRequest {
        public WebResponse Response { get; set; }
        public Task SendAsync();
        public void OnComplete(WebResponse response);
    }
}
