using Static;
using System;

public sealed class Player : Entity {

    public ItemType[] SlotTypes;
    public override void Init() {
        base.Init();

        SlotTypes = new ItemType[Inventory.Length];
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
