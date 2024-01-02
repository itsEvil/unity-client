using Account;
using Data;
using Game;
using Game.Controllers;
using Static;
using System;
using UnityEngine;
namespace Game.Entities {
    public sealed class Player : Entity {
        private const float _MIN_MOVE_SPEED = 0.004f;
        private const float _MAX_MOVE_SPEED = 0.0096f;
        private const float _MIN_ATTACK_FREQ = 0.0015f;
        private const float _MAX_ATTACK_FREQ = 0.008f;
        private const float _MIN_ATTACK_MULT = 0.5f;
        private const float _MAX_ATTACK_MULT = 2f;
        private const float _MAX_SINK_LEVEL = 18f;
        public float MoveMultiplier = 1f;
        public float PushX;
        public float PushY;


        public const int MaxLevel = 20;

        public bool IsMyPlayer = false;

        public int CurrentExp;
        public int NextLevelExp;
        public int Level;

        public int CurrentFame;
        public int NextFameGoal;

        public int Fame;
        public int Credits;
        public int NumStars;
        public string GuildName = string.Empty;
        public int GuildRank;
        public bool HasBackpack;
        public short SkinType;
        public ItemType[] SlotTypes;

        //Stats
        public int Mp;
        public int MaxMp;
        public int Attack;
        public int Defense;
        public int Speed;
        public int Dexterity;
        public int Vitality;
        public int Wisdom;

        private PlayerInputController InputController;

        /// <summary>
        /// Runs when any player gets initalized
        /// </summary>
        public override void Init(ObjectDesc descriptor, ObjectDefinition definition, bool isMyPlayer = false) {
            base.Init(descriptor, definition, isMyPlayer);
            IsMyPlayer = isMyPlayer;
            Type = GameObjectType.Player;

            var plrDescriptor = AssetLibrary.GetPlayerDesc(descriptor.Type);
            if (plrDescriptor == null)
                return;

            Inventory = new ushort[plrDescriptor.Equipment.Length];
            SlotTypes = new ItemType[Inventory.Length];
            for (int i = 0; i < SlotTypes.Length; i++)
                SlotTypes[i] = plrDescriptor.SlotTypes[i];

        }
        /// <summary>
        /// Only called when our player connects
        /// </summary>
        public void OnMyPlayerConnected() { 
            _moveController = new PlayerMoveController(this);
            //var charStats = AccountData.Characters[AccountData.CurrentCharId];
            //Inventory = charStats.Inventory;
            IsMyPlayer = true;
            InputController = new();
        }
        public override void UpdateStat(StatType stat, object value) {
            base.UpdateStat(stat, value);
            switch (stat) {
                case StatType.Experience:
                    CurrentExp = (int)value;
                    return;
                case StatType.ExperienceGoal:
                    NextLevelExp = (int)value;
                    return;
                case StatType.Level:
                    Level = (int)value;
                    return;
                case StatType.Fame:
                    //account fame ?
                    Fame = (int)value;
                    return;
                case StatType.Stars:
                    NumStars = (int)value;
                    return;
                case StatType.GuildName:
                    GuildName = (string)value;
                    return;
                case StatType.GuildRank:
                    GuildRank = (int)value;
                    return;
                case StatType.Credits:
                    Credits = (int)value;
                    return;
                case StatType.HasBackpack:
                    HasBackpack = (int)value == 1;
                    return;
                case StatType.MP:
                    Mp = (int)value;
                    return;
                case StatType.MaximumMP:
                    MaxMp = (int)value;
                    return;
                case StatType.Attack:
                    Attack = (int)value;
                    return;
            }
        }
        public float GetMovementSpeed() {
            if (HasEffect(ConditionEffectIndex.Paralyzed)) {
                Utils.Log("We are paralyzed!");
                return 0;
            }

            if (HasEffect(ConditionEffectIndex.Slowed)) {
                Utils.Log("We are slowed!");
                return _MIN_MOVE_SPEED; // * MoveMultiplier
            }

            var ret = _MIN_MOVE_SPEED + Speed / 75.0f * (_MAX_MOVE_SPEED - _MIN_MOVE_SPEED);

            if (HasEffect(ConditionEffectIndex.Speedy)) {
                Utils.Log("We are speedy!");
                return ret * 1.5f;
            }

            //MoveMultiplier is 1 always??
            //ret *= MoveMultiplier;

            return ret;
        }
        public override bool Tick() {
            InputController?.Tick();
            return base.Tick();
        }
        public void OnMove() {
            var tile = Map.Instance.GetTile((int)Position.x, (int)Position.y);
            if (tile == null) {
                Utils.Error("Tile at {0} {1} is null", Position.x, Position.y);
                return;
            }
            if (tile.Descriptor.Sinking) {
                SinkLevel = (int)Mathf.Min(SinkLevel + 1, _MAX_SINK_LEVEL);
                MoveMultiplier = 0.1f + (1 - SinkLevel / _MAX_SINK_LEVEL) * (tile.Descriptor.Speed - 0.1f);
            }
            else {
                SinkLevel = 0;
                MoveMultiplier = tile.Descriptor.Speed;
            }

            if (tile.Descriptor.Damage > 0 /* && !IsInvicible()*/) {
                if (tile.StaticObject == null || !tile.StaticObject.Descriptor.ProtectFromGroundDamage) {
                    Damage(tile.Descriptor.Damage, new ConditionEffectDesc[0], null);
                    //TODO damage player
                }
            }

            if (tile.Descriptor.Push) {
                PushX = tile.Descriptor.DX;
                PushX = tile.Descriptor.DY;
            }
            else {
                PushX = 0;
                PushY = 0;
            }
        }
        public override void Dispose() {
            base.Dispose();
        }
    }
}