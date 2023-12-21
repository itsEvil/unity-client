using Static;
namespace Game.Controllers {
    public interface IMoveController {
        public Vec2 Direction { get; }
        public abstract void Tick(float deltaTime);
    }
}
