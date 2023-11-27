using Networking;
using Networking.Tcp;
using Static;
using System;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace Game
{
    public partial class Map : MonoBehaviour
    {
        public static Map Instance;
        public static MapInfo MapInfo;

        public static Player MyPlayer;
        public static int GotosRequested;
        public static int MovesRequested;

        public static List<Entity> ToAddEntities = new();
        public static List<Entity> ToRemoveEntities = new();
        public static Dictionary<int, Entity> Entities = new();
        public static Dictionary<int, Interactive> Interactives = new();
        
        public static List<FPTimer> Timers = new();
        public static List<FPTimer> ToAddTimers = new();
        public static List<FPTimer> ToRemoveTimers = new();

        public static EntityPool EntityPool;
        public void Awake() {
            Instance = this;

            //TODO Add EntityPrefabs
            //EntityPool = new();
        }
        public void Init(MapInfo mapInfo) {
            MapInfo = mapInfo;
        }
        
        public void OnMyPlayerConnected(Player player) {
            MyPlayer = player;
            
            //Hide preloader
        }

        private void Update() {
            Add();
            Tick();
            Remove();
        }
        public void AddEntity(int type, int objectId, Vec2 position) {
            //TODO Add AssetLibrary
            //var desc = AssetLibrary.Type2Object[type];
            //var ent = EntityPool.Get(desc.GameObject);
            //ent.Id = objectId;
            //ent.Position = position;
            //ToAddEntities.Add(ent);
        }
        private void Add() {
            for (int i = 0; i < ToAddEntities.Count; i++) {
                var ent = ToAddEntities[i];
                ent.Init();
                Entities[ent.Id] = ent;
            }

            for(int i = 0; i < ToAddTimers.Count; i++) {
                Timers.Add(ToAddTimers[i]);
            }

            ToAddEntities.Clear();
            ToAddTimers.Clear();
        }
        private void Tick() {
            foreach(var (id, ent) in Entities) {
                if (ent == null || !ent.Tick())
                    ToRemoveEntities.Add(ent);
            }

            foreach (var timer in Timers) {
                //TODO Add Token source
                //if (_tokenSource.IsCancellationRequested)
                //    return;

                if (timer.TimeMS <= 0) {
                    timer.Action();
                    ToRemoveTimers.Add(timer);
                    continue;
                }

                timer.TimeMS -= GameTime.DeltaTime;
            }

            TickInteractives();

            if(MyPlayer != null) {
                TickMyPlayer();
            }
        }

        private void TickInteractives() {
            //TODO Get nearest interactive entity and enable its UI if its close enough
        }

        private void TickMyPlayer() {
            //while (GotosRequested > 0) {
            //    TcpTicker.Send(new GotoAck(GameTime.Time));
            //    GotosRequested--;
            //}

            while (MovesRequested > 0) {
                TcpTicker.Send(new Move(GameTime.Time, PacketHandler.Instance.TickTime, MyPlayer.Position.x, MyPlayer.Position.y, PacketHandler.Instance.History));
                MyPlayer.OnMove();
                MovesRequested--;
            }
        }

        private void Remove() {
            for (int i = 0; i < ToRemoveEntities.Count; i++) {

                var ent = ToRemoveEntities[i];

                Entities.Remove(ent.Id);
                if (ent.IsInteractive)
                    Interactives.Remove(ent.Id);

                //TODO Return Entity to Pool
            }

            for (int i = 0; i < ToRemoveTimers.Count; i++) {
                Timers.Remove(ToRemoveTimers[i]);
            }

            ToRemoveTimers.Clear();
            ToRemoveEntities.Clear();
        }
    }
}
