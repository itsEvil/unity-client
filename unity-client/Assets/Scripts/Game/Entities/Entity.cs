using Static;
using System;
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

    public ushort[] Inventory;

    public virtual void Init() {
        Inventory = new ushort[8];
    }
    public virtual bool Tick() {
        return !Dead;
    }
    public virtual void Dispose() {

    }
}
