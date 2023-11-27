using System.Threading.Tasks;

namespace Networking.Web
{
    public interface IWebRequest
    {
        public Task SendAsync();
    }
}
