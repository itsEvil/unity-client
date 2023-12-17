using Data;
using Networking;
using Networking.Tcp;
using Static;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using TileData = Static.TileData;

namespace Game {
    public partial class Map : MonoBehaviour {
        private const int BUFFER_SIZE_SMALL = 16;
        private const int BUFFER_SIZE_MED = 32;
        private const int BUFFER_SIZE_BIG = 64;
        private const int BUFFER_SIZE_LARGE = 128;

        [SerializeField] private Tilemap _tileMap;
        [SerializeField] private Transform _entityContainer;

        public static Map Instance;
        public static MapInfo MapInfo;

        public static Square[,] Tiles;

        public static Player MyPlayer;
        public static int GotosRequested;
        public static int MovesRequested;

        public static List<Entity> ToAddEntities = new(BUFFER_SIZE_MED);
        public static List<Entity> ToRemoveEntities = new(BUFFER_SIZE_MED);
        public static Dictionary<int, Entity> Entities = new(BUFFER_SIZE_LARGE);
        public static Dictionary<int, Interactive> Interactives = new(BUFFER_SIZE_MED);
        
        public static List<FPTimer> Timers = new(BUFFER_SIZE_MED);
        public static List<FPTimer> ToAddTimers = new(BUFFER_SIZE_SMALL);
        public static List<FPTimer> ToRemoveTimers = new(BUFFER_SIZE_SMALL);

        public static EntityPool EntityPool;
        public void Awake() {
            Instance = this;

            var prefabs = new Dictionary<GameObjectType, Entity>();
            foreach (var entity in Resources.LoadAll<Entity>("Prefabs/Entities")) {
                Utils.Log("Loaded prefab: {0}", entity.name);
                prefabs[Enum.Parse<GameObjectType>(entity.name)] = entity;
            }
            EntityPool = new(prefabs, _entityContainer);
        }
        public void OnEnable() {
            Dispose();
        }

        public void Init(MapInfo mapInfo) {
            MapInfo = mapInfo;
            Tiles = new Square[MapInfo.Width, MapInfo.Height];
        }
        
        public void OnMyPlayerConnected(Player player) {
            MyPlayer = player;
            MyPlayer.OnMyPlayer();
            //Hide preloader
        }

        private void Update() {
            Add();
            Tick();
            Remove();
        }
        public void SetTile(TileData data) {
            var tile = ScriptableObject.CreateInstance<Square>();
            var tileDesc = AssetLibrary.GetTileDesc(data.TileType);
            tile.Init(tileDesc, data.X, data.Y);
            Tiles[data.X, data.Y] = tile;
            _tileMap.SetTile(new Vector3Int(tile.X, tile.Y), tile);
        }

        public Square GetTile(int x, int y, bool checkBounds = false) {
            if(checkBounds) {
                if (x < 0 || y < 0)
                    return null;

                if(x >= MapInfo.Width || y >= MapInfo.Height) 
                    return null;
            }

            return Tiles[x, y];
        }
        public void AddEntity(Entity entity) {
            ToAddEntities.Add(entity);
        }
        private void Add() {
            for (int i = 0; i < ToAddEntities.Count; i++) {
                var ent = ToAddEntities[i];
                ent.AddToWorld();
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

        private void Dispose() {
            foreach(var (id, ent) in Entities) {
                Destroy(ent);
            }

            Entities.Clear();
            Interactives.Clear();
            
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
                var pos = MyPlayer.GetPosition();
                TcpTicker.Send(new Move(GameTime.Time, PacketHandler.Instance.TickTime, pos.x, pos.y, PacketHandler.Instance.History));
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
