using Static;
namespace Game.Entities {
    public sealed class Projectile : Entity {
        public override void Init(ObjectDesc descriptor, ObjectDefinition definition) {
            base.Init(descriptor, definition);
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