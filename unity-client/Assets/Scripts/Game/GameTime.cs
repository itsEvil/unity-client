﻿namespace Game {
    public static class GameTime {
        public static int Time => (int)(UnityEngine.Time.time * 1000f);
        public static int DeltaTime => (int)(UnityEngine.Time.deltaTime * 1000f);

        public static int TotalTicks = 0;
    }
}
