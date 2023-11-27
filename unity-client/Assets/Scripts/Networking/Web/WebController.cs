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
        public static Queue<WebWork> WorkQueue = new();//Remember to overrite this data when you send inventory action requests (InvSwap, InvDrop etc)
        private static readonly List<Task> _tasksList = new(16);
        private static CancellationTokenSource _tokenSource = new();
        /// <summary>
        /// Called on MapInfo/Map Init
        /// </summary>
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
            _tasksList.Clear();

            if (_tokenSource.Token.IsCancellationRequested)
                return;

            while (WorkQueue.TryDequeue(out var work)) {

                if (_tokenSource.Token.IsCancellationRequested)
                    return;

                _tasksList.Add(work.Handler.SendAsync());
            }

            if (_tokenSource.Token.IsCancellationRequested)
                return;

            await Task.WhenAll(_tasksList);
        }
        public static void Stop() {
            _tokenSource.Cancel();
        }
    }
    public readonly struct WebWork
    {
        public readonly IWebRequest Handler;
        public WebWork(IWebRequest handler)
        {
            Handler = handler;
        }
    }
}
