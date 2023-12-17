using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Networking.Web {
    public class CharDeleteHandler : IWebRequest {
        public WebResponse Response { get; set; }
        public Task SendAsync() {
            return Task.CompletedTask;
        }
        public void OnComplete(WebResponse response) { 
        
        }
    }
}
