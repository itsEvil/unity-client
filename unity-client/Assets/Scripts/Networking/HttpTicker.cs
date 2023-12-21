using Networking.Web;
using System;
using System.Threading.Tasks;
namespace Networking {
    public static class HttpTicker {
        private static Task _tickingTask;
        private static bool _crashed;
        public static bool Running => _tickingTask != null && !_tickingTask.IsCompleted;
        public static void Start() {
            WebController.Init();

            if (Running) {
                Utils.Warn("HttpTicker already started");
                return;
            }
            _crashed = false;
            _tickingTask = Task.Run(Tick);
        }
        private static void Tick() {
            while(Running) {
                //Utils.Log("HttpTicker::Tick");
                try {
                    WebController.Tick();
                }
                catch(Exception ex) {
                    _crashed = true;
                    Utils.Error("HttpTicker::", ex.Message, ex.StackTrace);
                    return;
                }
            }
        }
        public static void Stop() {
            WebController.Stop();
            _tickingTask = null;
            _crashed = false;
            Utils.Log("Http thread stopped!");
        }
    }
}