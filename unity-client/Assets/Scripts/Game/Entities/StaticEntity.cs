using Game.Controllers;
using Game.Models;
using Static;

namespace Game.Entities {
    public sealed class StaticEntity : Entity {
        private Wall Wall;
        public override void Init(ObjectDesc descriptor, ObjectDefinition definition) {
            base.Init(descriptor, definition);
            Type = GameObjectType.Static;

            if (Descriptor.Class == ObjectType.Wall) {
                CameraController.Instance.RemoveRotatingEntity(this);
                Wall = Map.Instance.GetWall(this, descriptor);
                Renderer.gameObject.SetActive(false);
            }
            
            CheckNearby();
        }
        public override bool Tick() {
            CheckNearby();

            return base.Tick();
        }
        public override void Dispose() {
            if(Wall != null)
                Destroy(Wall.gameObject);
            
            base.Dispose();
        }
        public void CheckNearby() {
            if (Descriptor.Class == ObjectType.Wall && Wall != null && !Wall.Satisfied)
                Wall.CheckNearbyWalls(Position);
        }
        public override bool MoveTo(Vec2 pos) {
            var oldTile = Map.Instance.GetTile((int)Position.x, (int)Position.y, true);

            if (oldTile != null)
                oldTile.StaticObject = null;

            var newTile = Map.Instance.GetTile((int)pos.x, (int)pos.y, true);
            if (newTile != null)
                newTile.StaticObject = this;

            return base.MoveTo(pos);
        }
    }
}