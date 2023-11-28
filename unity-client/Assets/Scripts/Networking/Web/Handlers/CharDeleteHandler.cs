using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Web {
    public class CharDeleteHandler : IWebRequest {
        public void Enqueue() {
            WebController.WorkQueue.Enqueue(new WebWork(this));
        }
        public Task SendAsync() {
            return Task.CompletedTask;
        }
    }
}
