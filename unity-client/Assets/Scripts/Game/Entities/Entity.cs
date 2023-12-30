using Data;
using Game;
using Game.Controllers;
using Static;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Entities
{


    public class Entity : MonoBehaviour, IDisposable
    {
        //Cache Transform as its an Extern call which is slow
        private Transform _transform;
        public Transform Transform {
            get
            {
                if (_transform == null)
                    _transform = transform;
                return _transform;
            }
        }
        [SerializeField] protected SpriteRenderer Renderer;
        public GameObjectType Type = GameObjectType.Entity;

        public string Name;
        public int Id;
        public bool Dead;
        public bool IsInteractive;
        public Vec2 Position
        {
            get => new(Transform.position.x, Transform.position.y);
            set
            {
                var yOffset = Descriptor.DrawOnGround ? -0.5f : 0f;
                Transform.position = new Vector3(value.x, value.y + yOffset, -Z);
            }
        }
        public float Z;
        protected int Hp;
        protected int MaxHp;
        protected int Size;
        protected int SinkLevel;
        protected ushort[] Inventory;
        public ObjectDesc Descriptor;
        public Square Square;
        protected IMoveController _moveController;
        public virtual void Init(ObjectDesc descriptor, ObjectDefinition definition) {
            Descriptor = descriptor;
            Name = Descriptor.DisplayId;
            Id = definition.ObjectStatus.Id;
            Position = definition.ObjectStatus.Position;
            Inventory = new ushort[8];

            Renderer.gameObject.SetActive(true);
            Renderer.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            Renderer.sprite = descriptor.TextureData.GetTexture(0);

            if (Id != PacketHandler.Instance.PlayerId)
                _moveController = new EntityMoveController(this);

            if (!Descriptor.DrawOnGround)
                CameraController.Instance.AddRotatingEntity(this);

        }
        public virtual void AddToWorld() {
            gameObject.SetActive(true);
        }
        public void OnNewTick(Vec2 position) {
            var movement = _moveController as EntityMoveController;
            if (movement!.TargetPosition == position)
                return;

            movement.TargetPosition = position;
            movement.Direction = (movement.TargetPosition - Position) / 127f;
        }
        public virtual bool Tick() {
            _moveController?.Tick(GameTime.DeltaTime);

            return !Dead;
        }
        public virtual void Damage(int damage, ConditionEffectDesc[] effects, Projectile projectile) {
            if (damage == 0 && effects.Length == 0)
                return;

            if (effects != null && effects.Length > 0) {
                AddStatusEffects(effects);
            }

            if (damage > 0) {
                Dead = Hp <= 0 && !Descriptor.Static;

                int lifeTimeMS = 1000;
                Color color = Color.red;

                //TODO add
                //Map.AddDamageText(this, damage, color, lifeTimeMS);
            }

            if (Descriptor.Enemy) {
                //TODO add
                //Map.SetDamageCounter(this);
            }
        }
        //TODO add
        private void AddStatusEffects(ConditionEffectDesc[] effects) {
            Utils.Log("Adding status effects ");
        }

        public virtual void Draw() {

        }
        public virtual void Dispose() {
            gameObject.SetActive(true);
        }
        public virtual bool MoveTo(Vec2 pos) {
            Map.Instance.MoveEntity(this, pos);
            return true;
        }
        //TODO add effects
        public virtual bool HasEffect(ConditionEffectIndex effect) {
            return false;
        }
        public void UpdateObjectStats(Dictionary<StatType, object> stats) {
            //Try automatically removed when game gets compiled
#if UNITY_EDITOR
            try
            {
#endif
                foreach (var stat in stats)
                    UpdateStat(stat.Key, stat.Value);
#if UNITY_EDITOR
            }
            catch (Exception e)
            {
                Utils.Error($"[Entity.Stats] {e.Message}\n{e.StackTrace}");
                foreach (var stat in stats)
                    Utils.Log($"[Stat] {stat.Key} {stat.Value}");
            }
#endif
        }
        public virtual void UpdateStat(StatType stat, object value) {
            switch (stat) {
                case StatType.MaximumHP:
                    MaxHp = (int)value;
                    return;
                case StatType.HP:
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
            var descriptor = AssetLibrary.GetObjectDesc(definition.ObjectType);
            if (definition.ObjectStatus.Id == PacketHandler.Instance.PlayerId) {
                Map.MyPlayer.Init(descriptor, definition);
                Map.MyPlayer.UpdateObjectStats(definition.ObjectStatus.Stats);
                Map.Instance.OnMyPlayerConnected();
                return Map.MyPlayer;
            }

            var entity = Map.Instance.EntityPool.Get(descriptor.ObjectClass);
            entity.Init(descriptor, definition);
            entity.UpdateObjectStats(definition.ObjectStatus.Stats);

            return entity;
        }
        public ushort[] GetInventory() => Inventory;
        public int GetHp() => Hp;
        public int GetMaxHp() => MaxHp;
        public void SetPosition(Vec2 position) => Position = position;
    }
}