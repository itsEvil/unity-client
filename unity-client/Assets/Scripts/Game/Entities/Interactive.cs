using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : Entity
{
    public override void Init() {
        base.Init();

        IsInteractive = true;
        Type = Static.GameObjectType.Interactive;
    }
    public override bool Tick() {
        return base.Tick();
    }
    public override void Dispose() {
        base.Dispose();
    }
}
