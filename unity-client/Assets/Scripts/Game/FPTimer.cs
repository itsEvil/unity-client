using System;
namespace Game {
    public sealed class FPTimer {
        public int TimeMS;
        public Action Action;
        public FPTimer(int time, Action action) {
            TimeMS = time;
            Action = action;
        }
    }
}
