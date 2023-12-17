using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Networking.Web
{
    public sealed partial class WebController
    {
        public static Queue<WebWork> WorkQueue = new();
        private static readonly List<Task> _tasksList = new(16);
        private static CancellationTokenSource _tokenSource = new();
        private static List<WebWork> _queueToReAdd = new(16);
        public static void Init() {
            _tokenSource.Cancel();
            WorkQueue.Clear();
            _tasksList.Clear();
            _tokenSource.Dispose();
            _tokenSource = new();
        }
        /// <summary>
        /// Called ONLY on Network thread
        /// </summary>
        public static async void Tick() {
            if (WorkQueue.Count == 0)
                return;

            _tasksList.Clear();
            _queueToReAdd.Clear();

            if (_tokenSource.Token.IsCancellationRequested)
                return;

            while (WorkQueue.TryDequeue(out var work)) {

                if (_tokenSource.Token.IsCancellationRequested)
                    return;

                _tasksList.Add(work.Handler.SendAsync());
                _queueToReAdd.Add(work);
            }

            if (_tokenSource.Token.IsCancellationRequested)
                return;

            Utils.Log("Awaiting {0} WebTasks", _tasksList.Count);
            await Task.WhenAll(_tasksList);
            Utils.Log("Done awaiting all WebTasks");

            Utils.Log("Handling all responses");
            foreach(var work in _queueToReAdd)
                HttpHandler.ToBeHandled.Enqueue(work);

            Utils.Log("HttpHandler.ToBeHandled.Count: {0}", HttpHandler.ToBeHandled.Count);
        }
        public static void Stop() {
            _tokenSource.Cancel();
        }
    }
    public readonly struct WebWork
    {
        public readonly IWebRequest Handler;
        public WebWork(IWebRequest handler) {
            Handler = handler;
        }
    }
}
