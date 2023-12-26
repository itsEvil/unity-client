using Static;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class StaticEntity : Entity
{
    public override void Init(ObjectDesc desc, ObjectDefinition defi) {
        base.Init(desc, defi);
        Type = GameObjectType.Static;
    }
    public override bool Tick() {
        return base.Tick();
    }
    public override void Dispose() {
        base.Dispose();
    }
}
