using Account;
using Data;
using Game.Controllers;
using Game.Entities;
using Game.Models;
using Networking;
using Networking.Tcp;
using Static;
using System;
using System.Collections.Generic;
using System.Threading;
using UI;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;
using static UnityEngine.EventSystems.EventTrigger;
using TileData = Static.TileData;

namespace Game {
    public partial class Map : MonoBehaviour {
        private const int BUFFER_SIZE_SMALL = 16;
        private const int BUFFER_SIZE_MED = 32;
        private const int BUFFER_SIZE_BIG = 64;
        private const int BUFFER_SIZE_LARGE = 128;
        private const int INTERACTIVE_UPDATE_INTERVAL = 50;
        private const float MAXIMUM_INTERACTION_DISTANCE = 1f;

        [SerializeField] private Tilemap _tileMap;
        [SerializeField] private Transform _entityContainer;
        [SerializeField] public EntityPool EntityPool;
        [SerializeField] private Wall _wallPrefab; 
        public static Map Instance;
        public static MapInfo MapInfo;
        public static CreateSuccess CreateSuccess;
        public static bool UseCreatePosition;

        public static Square[,] Tiles;

        public static Player MyPlayer;
        public static int GotosRequested;
        public static int MovesRequested;

        public static List<Entity> ToAddEntities = new(BUFFER_SIZE_MED);
        public static List<int> ToRemoveEntities = new(BUFFER_SIZE_MED);
        public static Dictionary<int, Entity> Entities = new(BUFFER_SIZE_LARGE);
        public static Dictionary<int, Interactive> Interactives = new(BUFFER_SIZE_MED);
        
        public static List<FPTimer> Timers = new(BUFFER_SIZE_MED);
        public static List<FPTimer> ToAddTimers = new(BUFFER_SIZE_SMALL);
        public static List<FPTimer> ToRemoveTimers = new(BUFFER_SIZE_SMALL);
        public static CancellationTokenSource TokenSource;

        private static List<Interactive> NearbyInteractives = new(BUFFER_SIZE_SMALL);
        private static Interactive NearestInteractive;
        private static int LastInteractiveUpdateTime;

        public void Awake() {
            Instance = this;
        }
        public void OnEnable() {
            Dispose();
        }
        public void OnDisable() {
            Dispose();
        }
        public void Init(MapInfo mapInfo) {
            Dispose();
            MapInfo = mapInfo;
            Tiles = new Square[MapInfo.Width, MapInfo.Height];
            GameScreenController.Instance.OnMapInfo(mapInfo);
        }
        public void OnCreateSuccess(CreateSuccess create) {
            CreateSuccess = create;
            UseCreatePosition = true;
            PacketHandler.Instance.PlayerId = create.ObjectId;
            AccountData.CurrentCharId = create.CharacterId;
            
            if(MyPlayer == null) {
                var plr = EntityPool.Get(GameObjectType.Player) as Player;
                plr.Id = create.ObjectId;
                MyPlayer = plr;
            }
        }
        public void OnMyPlayerConnected() {
            MyPlayer.Position = new Vec2(CreateSuccess.Position.x, CreateSuccess.Position.y);
            Utils.Log("MyPlayerConnected! {0} at {1}", MyPlayer.Name, MyPlayer.Position);
            CameraController.Instance.SetFocus(MyPlayer.gameObject);
            GameScreenController.Instance.OnMyPlayerConnected(MyPlayer);
            UseCreatePosition = false;
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

            if (tile == null)
                return;

            if (entity.Descriptor.Static) {
                if (entity.Square != null) {
                    entity.Square.StaticObject = null;
                }
                tile.StaticObject = entity as StaticEntity;
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
        public void PrepareRemoveEntity(Entity ent) {
            ToRemoveEntities.Add(ent.Id);
        }
        public void PrepareRemoveEntity(int id) {
            ToRemoveEntities.Add(id);
        }
        public void AddEntity(Entity entity) {
            ToAddEntities.Add(entity);
        }
        private void Add() {
            for (int i = 0; i < ToAddEntities.Count; i++) {
                var ent = ToAddEntities[i];
                ent.AddToWorld();
                Entities[ent.Id] = ent;

                if (ent.IsInteractive)
                    Interactives[ent.Id] = ent as Interactive;

                if (ent.Descriptor.Static) {
                    var tile = GetTile((int)ent.Position.x, (int)ent.Position.y, true);
                    if (tile == null)
                        continue;

                    tile.StaticObject = ent as StaticEntity;
                }
            }

            for(int i = 0; i < ToAddTimers.Count; i++)
                Timers.Add(ToAddTimers[i]);

            ToAddEntities.Clear();
            ToAddTimers.Clear();
        }
        private void Remove() {
            for (int i = 0; i < ToRemoveEntities.Count; i++) {
                var objectId = ToRemoveEntities[i];

                Utils.Log("Trying to remove ent {0}", objectId);

                if(!Entities.TryGetValue(objectId, out var ent))
                    continue;

                CameraController.Instance.RemoveRotatingEntity(ent);

                if (ent.IsInteractive)
                    Interactives.Remove(objectId);

                if (ent.Descriptor.Static) {
                    var tile = GetTile((int)ent.Position.x, (int)ent.Position.y, true);
                    if (tile == null)
                        continue;

                    tile.StaticObject = null;
                }

                Utils.Log("Removed ent {0} {1}", ent.Descriptor.Id, objectId);

                EntityPool.Return(ent);
                Entities.Remove(objectId);
            }

            for (int i = 0; i < ToRemoveTimers.Count; i++)
                Timers.Remove(ToRemoveTimers[i]);
            

            ToRemoveTimers.Clear();
            ToRemoveEntities.Clear();
        }
        private void Tick() {
            foreach(var (_, ent) in Entities) {
                if (TokenSource.IsCancellationRequested)
                    return;

                if (ent == null || !ent.Tick())
                    PrepareRemoveEntity(ent.Id);
            }

            foreach (var timer in Timers) {
                if (TokenSource.IsCancellationRequested)
                    return;

                if (timer.TimeMS <= 0) {
                    timer.Action();
                    ToRemoveTimers.Add(timer);
                    continue;
                }

                timer.TimeMS -= GameTime.DeltaTime;
            }

            TickInteractives();

            if (MyPlayer != null)
                MyPlayer.OnMove();
        }

        public void Dispose() {
            TcpTicker.ClearSend();
            TokenSource?.Cancel();
            GameScreenController.Instance.SetAllOptionalWidgetVisibility(false);
            _tileMap.ClearAllTiles();
            CameraController.Instance.Clear();
            MyPlayer = null;
            foreach (var (_, ent) in Entities)
                Destroy(ent.gameObject);
            

            TokenSource = new();
            EntityPool.Clear();
            Entities.Clear();
            Interactives.Clear();
            ToAddEntities.Clear();
            ToRemoveEntities.Clear();
        }

        private void TickInteractives() {
            //TODO Get nearest interactive entity and enable its UI if its close enough
            if (GameTime.Time - LastInteractiveUpdateTime >= INTERACTIVE_UPDATE_INTERVAL && MyPlayer != null) {
                //Utils.Log("Ticking interactives {0} {1}", Interactives.Count, TokenSource.IsCancellationRequested);
                UpdateNearestInteractive();
                LastInteractiveUpdateTime = GameTime.Time;
            }
        }

        private void UpdateNearestInteractive() {
            var minDistSqr = MAXIMUM_INTERACTION_DISTANCE * MAXIMUM_INTERACTION_DISTANCE;
            var playerX = MyPlayer.Position.x;
            var playerY = MyPlayer.Position.y;
            Interactive closestInteractive = null;
            NearbyInteractives.Clear();
            int count = 0;

            foreach (var (_, obj) in Interactives) {
                if (TokenSource.IsCancellationRequested) {
                    if(NearestInteractive != null)
                        NearestInteractive.SetWidgetVisibility(false);
                    
                    return;
                }

                var objX = obj.transform.position.x;
                var objY = obj.transform.position.y;
                if (Mathf.Abs(playerX - objX) < MAXIMUM_INTERACTION_DISTANCE &&
                    Mathf.Abs(playerY - objY) < MAXIMUM_INTERACTION_DISTANCE) {
                    NearbyInteractives.Add(obj);
                }
            }

            foreach (var obj in NearbyInteractives) {
                if (TokenSource.IsCancellationRequested) {
                    if(NearestInteractive != null)
                        NearestInteractive.SetWidgetVisibility(false);
                    
                    return;
                }

                count++;
                var objX = obj.transform.position.x;
                var objY = obj.transform.position.y;
                if (Mathf.Abs(playerX - objX) < MAXIMUM_INTERACTION_DISTANCE &&
                    Mathf.Abs(playerY - objY) < MAXIMUM_INTERACTION_DISTANCE)
                {
                    var distSqr = FPMathUtils.DistanceSquared(MyPlayer.Position, obj.Position);
                    if (distSqr < minDistSqr) {
                        minDistSqr = distSqr;
                        closestInteractive = obj;
                    }
                }

            }

            //If we dont have any close interactives clear current
            if (closestInteractive == null) {
                if (NearestInteractive != null)
                    NearestInteractive.SetWidgetVisibility(false);
                
                NearestInteractive = null;
                return;
            }

            //If its the same entity dont do anythin
            if (NearestInteractive != null && closestInteractive != null)
                if (NearestInteractive.Id == closestInteractive.Id)
                    return;

            //Else turn current off and replace it with closest if available
            if (NearestInteractive != null)
                NearestInteractive.SetWidgetVisibility(false);

            NearestInteractive = closestInteractive;

            Utils.Log("Finished ticking interactives");
            
            if (NearestInteractive != null) {
                //Utils.Log("Nearest Interactive is {0}", NearestInteractive.Name);
                NearestInteractive.SetWidgetVisibility(true);
            }            
        }

        public void InteractWithNearby() {
            if (NearestInteractive != null)
                NearestInteractive.Interact();
        }

        public Wall GetWall(Entity entity, ObjectDesc descriptor) {
            var wall = Instantiate(_wallPrefab, entity.Transform);
            var pos = entity.Transform.position;
            wall.transform.position = new Vector3(pos.x, pos.y - 0.5f, -1f);
            wall.Init(descriptor); 
            return wall;
        }
    }
}
