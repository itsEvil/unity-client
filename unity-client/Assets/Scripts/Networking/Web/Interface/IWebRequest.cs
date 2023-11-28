using System.Threading.Tasks;

namespace Networking.Web
{
    public interface IWebRequest
    {
        public void Enqueue();
        public Task SendAsync();
    }
}
