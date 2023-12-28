using Static;
using System;

namespace Game.Controllers {
    public class EntityMoveController : IMoveController {
        public Vec2 Direction { get; set; }
        public Vec2 TargetPosition;

        private readonly Entity _entity;
        private readonly bool _interpolated;
        public EntityMoveController(Entity entity) {
            _entity = entity;
            Direction = Vec2.zero;
            TargetPosition = Vec2.zero;
            _interpolated = !_entity.Descriptor.Static;
        }
        //TODO
        //Turn this into a Job???
        public void Tick(float deltaTime) {
            var moving = false;
            if (Direction != Vec2.zero) {
                var position = _entity.Position;

                var direction = Direction;
                var dx = direction.x * deltaTime;
                var dy = direction.y * deltaTime;
                var nextX = position.x + dx;
                var nextY = position.y + dy;

                if (direction.x > 0 && nextX > TargetPosition.x ||
                    direction.x < 0 && nextX < TargetPosition.x)
                {
                    nextX = TargetPosition.x;
                    direction.x = 0;
                }

                if (direction.y > 0 && nextY > TargetPosition.y ||
                    direction.y < 0 && nextY < TargetPosition.y)
                {
                    nextY = TargetPosition.y;
                    direction.y = 0;
                }

                Direction = direction;
                _entity.MoveTo(new Vec2(nextX, nextY));
                moving = true;
            }

            //if (_entity.Descriptor.WhileMoving != null) {
            //    if (!moving) {
            //        _entity.Z = _entity.Desc.Z;
            //        _entity.Flying = _entity.Desc.Flying;
            //    }
            //    else {
            //        _entity.Z = _entity.Desc.WhileMoving.Z;
            //        _entity.Flying = _entity.Desc.WhileMoving.Flying;
            //    }
            //}
        }
    }
}
