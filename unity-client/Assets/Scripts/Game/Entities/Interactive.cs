using Static;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactive : Entity
{
    public override void Init(ObjectDesc desc) {
        base.Init(desc);
        Type = GameObjectType.Interactive;
    }
    public override bool Tick() {
        return base.Tick();
    }
    public override void Dispose() {
        base.Dispose();
    }
}
