using Game.Controllers;
using Static;
using UnityEngine;

namespace Game.Entities {
    public sealed class StaticEntity : Entity {
        public override void Init(ObjectDesc descriptor, int id, Vec2 position, bool isMyPlayer = false) {
            base.Init(descriptor, id, position, isMyPlayer);
            Type = GameObjectType.Static;
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