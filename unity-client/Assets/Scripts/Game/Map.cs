using Data;
using Game.Controllers;
using Networking;
using Networking.Tcp;
using Static;
using System;
using System.Collections.Generic;
using UI;
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
            GameScreenController.Instance.OnMapInfo();
        }
        
        public void OnMyPlayerConnected(Player player) {
            MyPlayer = player;
            MyPlayer.OnMyPlayer();
            Utils.Log("MyPlayerConnected! {0}", player.Name);
            CameraController.Instance.SetFocus(MyPlayer.gameObject);
            GameScreenController.Instance.OnMyPlayerConnected(MyPlayer);
            //Hide preloader
        }

        private void Update() {
            Add();
            Tick();
            Remove();
        }
        public Entity GetEntity(int id) {
            if(Entities.TryGetValue(id, out var entity)) 
                return entity;

            return null;
        }
        private Square GetSquare(int x, int y) {
            var tile = GetTile(x, y, true);
            if(tile == null) {
                var square = ScriptableObject.CreateInstance<Square>();
                Tiles[x, y] = square;
                return square;
            }
            return tile;
        }
        public void SetTiles(Vector3Int[] positions, ushort[] tileTypes) {
            Square[] squares = new Square[tileTypes.Length];
            Span<Vector3Int> positionSpan = positions.AsSpan();
            Span<ushort> tileTypeSpan = tileTypes.AsSpan();
            for(int i = 0; i < tileTypes.Length; i++) {
                var tile = tileTypeSpan[i];
                var pos = positionSpan[i];
                var square = GetSquare(pos.x, pos.y);
                square.Init(AssetLibrary.GetTileDesc(tile), pos.x, pos.y);
                squares[i] = square;
            }

            _tileMap.SetTiles(positions, squares);
        }
        public void SetTile(TileData data) {
            var tile = ScriptableObject.CreateInstance<Square>();
            var tileDesc = AssetLibrary.GetTileDesc(data.TileType);
            Utils.Log("Setting tile {0} at {1}:{2}", tileDesc.Id, data.X, data.Y);
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
        public void MoveEntity(Entity entity, Vec2 position) {
            var tile = GetTile((int)position.x, (int)position.y);
            entity.SetPosition(position);

            if (entity.Descriptor.Static) {
                if (entity.Square != null) {
                    entity.Square.StaticObject = null;
                }

                tile.StaticObject = entity;
            }
            entity.Square = tile;
        }
        public bool RegionUnblocked(float x, float y) {
            if (TileIsWalkable(x, y))
                return false;

            var xFrac = x - (int)x;
            var yFrac = y - (int)y;

            if (xFrac < 0.5f) {
                if (TileFullOccupied(x - 1, y))
                    return false;

                if (yFrac < 0.5f) {
                    if (TileFullOccupied(x, y - 1) || TileFullOccupied(x - 1, y - 1))
                        return false;
                }
                else {
                    if (yFrac > 0.5f)
                        if (TileFullOccupied(x, y + 1) || TileFullOccupied(x - 1, y + 1))
                            return false;
                }

                return true;
            }

            if (xFrac > 0.5f) {
                if (TileFullOccupied(x + 1, y))
                    return false;

                if (yFrac < 0.5) {
                    if (TileFullOccupied(x, y - 1) || TileFullOccupied(x + 1, y - 1))
                        return false;
                }
                else {
                    if (yFrac > 0.5)
                        if (TileFullOccupied(x, y + 1) || TileFullOccupied(x + 1, y + 1))
                            return false;
                }

                return true;
            }

            if (yFrac < 0.5) {
                if (TileFullOccupied(x, y - 1))
                    return false;

                return true;
            }

            if (yFrac > 0.5)
                if (TileFullOccupied(x, y + 1))
                    return false;

            return true;
        }

        private bool TileIsWalkable(float x, float y) {
            var tile = GetTile((int)x, (int)y);
            if (tile == null)
                return true;

            if (tile.Descriptor.NoWalk)
                return true;

            if (tile.StaticObject != null) {
                if (tile.StaticObject.Descriptor.OccupySquare)
                    return true;
            }

            return false;
        }

        private bool TileFullOccupied(float x, float y) {
            var tile = GetTile((int)x, (int)y);
            if (tile == null)
                return true;

            if (tile.Type == 255)
                return true;

            if (tile.StaticObject != null) {
                if (tile.StaticObject.Descriptor.FullOccupy)
                    return true;
            }

            return false;
        }
        public void RemoveEntity(int id) {
            if(Entities.TryGetValue(id, out var ent)) {
                ToRemoveEntities.Add(ent);
            }
            else {
                Utils.Warn("Entity with id {0} not found for removal", id);
            }
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

            if (MyPlayer != null) {
                MyPlayer.OnMove();
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

        private void Remove() {
            for (int i = 0; i < ToRemoveEntities.Count; i++) {

                var ent = ToRemoveEntities[i];

                EntityPool.Return(ent);
                Entities.Remove(ent.Id);
                CameraController.Instance.RemoveRotatingEntity(ent);

                if (ent.IsInteractive)
                    Interactives.Remove(ent.Id);
            }

            for (int i = 0; i < ToRemoveTimers.Count; i++) {
                Timers.Remove(ToRemoveTimers[i]);
            }

            ToRemoveTimers.Clear();
            ToRemoveEntities.Clear();
        }
    }
}
