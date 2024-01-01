using Game.Controllers;
using Static;
using UnityEngine;

namespace Game.Entities {
    public sealed class StaticEntity : Entity {
        private Model Model;
        public override void Init(ObjectDesc descriptor, ObjectDefinition definition) {
            base.Init(descriptor, definition);
            Type = GameObjectType.Static;

            if (Descriptor.Class == ObjectType.Wall) {
                CameraController.Instance.RemoveRotatingEntity(this);
                Model = Map.Instance.EntityPool.Get(GameObjectType.Model) as Model;
                Model.Init(descriptor, definition);
                Model.Transform.SetPositionAndRotation(new Vector3(Position.x, Position.y, -0.5f), Quaternion.Euler(0, 180, 0));
                Model.Transform.localScale = new Vector3(50, 50, 50);
                Renderer.gameObject.SetActive(false);
            }
        }
        public override void Dispose() {
            if(Model != null)
                Map.Instance.EntityPool.Return(Model);
            
            base.Dispose();
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