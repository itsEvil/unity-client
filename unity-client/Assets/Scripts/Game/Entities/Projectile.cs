using Static;
namespace Game.Entities {
    public sealed class Projectile : Entity {
        public override void Init(ObjectDesc descriptor, ObjectDefinition definition, bool isMyPlayer = false) {
            base.Init(descriptor, definition, isMyPlayer);
            Type = GameObjectType.Projectile;
        }
        public override bool Tick() {
            return base.Tick();
        }
        public override void Dispose() {
            base.Dispose();
        }
    }
}