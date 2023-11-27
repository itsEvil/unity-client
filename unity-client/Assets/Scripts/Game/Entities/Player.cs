using Static;

public sealed class Player : Entity {
    public override void Init() {
        base.Init();

        Type = GameObjectType.Player;
    }
    public override bool Tick() {
        return base.Tick();
    }
    public void OnMove() {

    }
    public override void Dispose() {
        base.Dispose();
    }
}
