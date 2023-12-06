using Data;
using Game;
using Static;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;
using UnityEngine;

public class Entity : MonoBehaviour, IDisposable {
    [SerializeField] private string _name;
    public GameObjectType Type = GameObjectType.Entity;
    public int Id;
    public bool Dead;
    public bool IsInteractive;
    public Vec2 Position;
    public int Hp;
    public int MaxHp;
    public int Size;
    public string Name;
    public int SinkLevel;

    public ushort[] Inventory;
    public ObjectDesc Desc;
    public virtual void Init(ObjectDesc desc) {
        Desc = desc;
        Inventory = new ushort[8];
    }
    public virtual void AddToWorld() {

    }
    public virtual bool Tick() {
        return !Dead;
    }
    public virtual void Dispose() {

    }
    public void UpdateObjectStats(Dictionary<StatType, object> stats) {
        //Try automatically removed when game gets compiled
#if UNITY_EDITOR
        try {
#endif
            foreach (var stat in stats)
                UpdateStat(stat.Key, stat.Value);
#if UNITY_EDITOR
        }
        catch (Exception e) {
            Utils.Error($"[Entity.Stats] {e.Message}\n{e.StackTrace}");
            foreach (var stat in stats)
                Utils.Log($"[Stat] {stat.Key} {stat.Value}");
        }
#endif
    }
    public virtual void UpdateStat(StatType stat, object value) {
        switch (stat) {
            case StatType.MaxHp:
                MaxHp = (int)value;
                return;
            case StatType.Hp:
                Hp = (int)value;
                return;
            case StatType.Size:
                Size = (int)value;
                return;
            case StatType.Name:
                Name = (string)value;
                return;
            case StatType.SinkLevel:
                SinkLevel = (int)value;
                return;
        }
    }
    public static Entity Resolve(ObjectDefinition definition) {
        var desc = AssetLibrary.GetObjectDesc((ushort)definition.ObjectType);
        var entity = Map.EntityPool.Get(desc.ObjectClass);
        entity.Init(desc);
        entity.UpdateObjectStats(definition.ObjectStatus.Stats);

        if (definition.ObjectStatus.Id == PacketHandler.Instance.PlayerId)
            Map.Instance.OnMyPlayerConnected(entity as Player);

        return entity;
    }
}
