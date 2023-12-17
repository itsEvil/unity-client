using Networking.Web;
using System.Collections.Concurrent;
using UnityEngine;
namespace Networking {
    public class HttpHandler : MonoBehaviour {
        public static ConcurrentQueue<WebWork> ToBeHandled = new();
        private void Start() {
            HttpTicker.Start();
        }
        private void OnApplicationQuit() {
            HttpTicker.Stop();
        }
        private void FixedUpdate() {
            if (ToBeHandled.Count == 0) 
                return;

            while(ToBeHandled.TryDequeue(out WebWork work)) {
                work.Handler.OnComplete(work.Handler.Response);
            }
        }
    }
}
