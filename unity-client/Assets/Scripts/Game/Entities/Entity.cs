using Data;
using Game;
using Static;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour, IDisposable {
    [SerializeField] protected SpriteRenderer Renderer;
    public GameObjectType Type = GameObjectType.Entity;

    public int Id;
    public bool Dead;
    public bool IsInteractive;
    protected Vec2 Position;
    protected int Hp;
    protected int MaxHp;
    protected int Size;
    protected string Name;
    protected int SinkLevel;
    protected ushort[] Inventory;
    protected ObjectDesc Desc;
    public virtual void Init(ObjectDesc desc) {
        Desc = desc;
        Inventory = new ushort[8];
        Renderer.sprite = desc.TextureData.GetTexture(0);
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
    public ushort[] GetInventory() => Inventory;
    public int GetHp() => Hp;
    public int GetMaxHp() => MaxHp;
    public void SetPosition(Vec2 position) => Position = position;
    public Vec2 GetPosition() => Position;
}
