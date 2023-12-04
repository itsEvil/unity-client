using UnityEngine;
namespace Networking {
    public class HttpThreadStarter : MonoBehaviour {
        private void Start() {
            HttpTicker.Start();
        }
        private void OnApplicationQuit() {
            HttpTicker.Stop();
        }
    }
}
