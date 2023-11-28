﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEditor;
using UnityEngine;

namespace Static
{
    public class ObjectDesc
    {
        public readonly string Id;
        public readonly ushort Type;

        public readonly string DisplayId;
        public readonly string Group;

        public readonly bool Static;
        public readonly string Class;
        public readonly bool BlocksSight;

        public readonly bool OccupySquare;
        public readonly bool FullOccupy;
        public readonly bool EnemyOccupySquare;

        public readonly bool ProtectFromGroundDamage;
        public readonly bool ProtectFromSink;

        public readonly bool Player;
        public readonly bool Enemy;

        public readonly bool God;
        public readonly bool Cube;
        public readonly bool Quest;
        public readonly bool Hero;
        public readonly int Level;
        public readonly bool Oryx;
        public readonly float XpMult;

        public readonly int Size;
        public readonly int MinSize;
        public readonly int MaxSize;

        public readonly int MaxHp;
        public readonly int Defense;

        public readonly string DungeonName;

        public readonly SpawnData SpawnData;
        public readonly int PerRealmMax;

        public readonly ProjectileDesc[] Projectiles;

        public ObjectDesc(XElement e, string id, ushort type)
        {
            Id = id;
            Type = type;

            DisplayId = e.ParseString("DisplayId", Id);
            Group = e.ParseString("Group");

            Static = e.ParseBool("Static");
            Class = e.ParseString("Class");
            BlocksSight = e.ParseBool("BlocksSight");

            OccupySquare = e.ParseBool("OccupySquare");
            FullOccupy = e.ParseBool("FullOccupy");
            EnemyOccupySquare = e.ParseBool("EnemyOccupySquare");

            ProtectFromGroundDamage = e.ParseBool("ProtectFromGroundDamage");
            ProtectFromSink = e.ParseBool("ProtectFromSink");

            Enemy = e.ParseBool("Enemy");
            Player = e.ParseBool("Player");

            God = e.ParseBool("God");
            Cube = e.ParseBool("Cube");
            Quest = e.ParseBool("Quest");
            Hero = e.ParseBool("Hero");
            Level = e.ParseInt("Level", -1);
            Oryx = e.ParseBool("Oryx");
            XpMult = e.ParseFloat("XpMult", 1);

            Size = e.ParseInt("Size", 100);
            MinSize = e.ParseInt("MinSize", Size);
            MaxSize = e.ParseInt("MaxSize", Size);

            MaxHp = e.ParseInt("MaxHitPoints");
            Defense = e.ParseInt("Defense");

            DungeonName = e.ParseString("DungeonName");

            if (e.Element("Spawn") != null)
                SpawnData = new SpawnData(e.Element("Spawn"));
            PerRealmMax = e.ParseInt("PerRealmMax");

            Projectiles = e.Elements("Projectile").Select(i => new ProjectileDesc(i, Type)).ToArray();
            /*
            Projectiles = new Dictionary<int, ProjectileDesc>();
            foreach (var k in e.Elements("Projectile"))
            {
                var desc = new ProjectileDesc(k, Type);
#if DEBUG
                if (Projectiles.ContainsKey(desc.BulletType))
                    throw new Exception("Duplicate bullet type");
#endif
                Projectiles[desc.BulletType] = desc;
            }*/
        }
    }

    public class SpawnData
    {
        public readonly int Mean;
        public readonly int StdDev;
        public readonly int Min;
        public readonly int Max;

        public SpawnData(XElement e)
        {
            Mean = e.ParseInt("Mean");
            StdDev = e.ParseInt("StdDev");
            Min = e.ParseInt("Min");
            Max = e.ParseInt("Max");
        }
    }

    public class PlayerDesc : ObjectDesc
    {
        public readonly ItemType[] SlotTypes;
        public readonly int[] Equipment;
        public readonly int[] ItemDatas;
        public readonly StatDesc[] Stats;
        public readonly int[] StartingValues;

        public PlayerDesc(XElement e, string id, ushort type) : base(e, id, type)
        {
            SlotTypes = e.ParseIntArray("SlotTypes", ",").Select(x => (ItemType)x).ToArray();

            var equipment = e.ParseUshortArray("Equipment", ",").Select(k => k == 0xffff ? -1 : k).ToArray();
            Equipment = new int[20];
            for (var k = 0; k < 20; k++)
                Equipment[k] = k >= equipment.Length ? -1 : equipment[k];

            ItemDatas = new int[20];
            for (var k = 0; k < 20; k++)
                ItemDatas[k] = -1;

            Stats = new StatDesc[8];
            for (var i = 0; i < Stats.Length; i++)
                Stats[i] = new StatDesc(i, e);
            Stats = Stats.OrderBy(k => k.Index).ToArray();

            StartingValues = Stats.Select(k => k.StartingValue).ToArray();
        }
    }

    public class StatDesc
    {
        public readonly string Type;
        public readonly int Index;
        public readonly int MaxValue;
        public readonly int StartingValue;
        public readonly int MinIncrease;
        public readonly int MaxIncrease;

        public StatDesc(int index, XElement e)
        {
            Index = index;
            Type = StatIndexToName(index);

            StartingValue = e.ParseInt(Type);
            MaxValue = e.Element(Type).ParseInt("@max");

            foreach (var stat in e.Elements("LevelIncrease"))
            {
                if (stat.Value == Type)
                {
                    MinIncrease = stat.ParseInt("@min");
                    MaxIncrease = stat.ParseInt("@max");
                    break;
                }
            }
        }

        public static string StatIndexToName(int index)
        {
            switch (index)
            {
                case 0: return "MaxHitPoints";
                case 1: return "MaxMagicPoints";
                case 2: return "Attack";
                case 3: return "Defense";
                case 4: return "Speed";
                case 5: return "Dexterity";
                case 6: return "HpRegen";
                case 7: return "MpRegen";
            }
            return null;
        }

        public static int StatNameToIndex(string name)
        {
            switch (name)
            {
                case "MaxHitPoints": return 0;
                case "MaxMagicPoints": return 1;
                case "Attack": return 2;
                case "Defense": return 3;
                case "Speed": return 4;
                case "Dexterity": return 5;
                case "HpRegen": return 6;
                case "MpRegen": return 7;
            }
            return -1;
        }
    }

    public class SkinDesc
    {
        public readonly string Id;
        public readonly ushort Type;

        public readonly ushort PlayerClassType;

        public SkinDesc(XElement e, string id, ushort type)
        {
            Id = id;
            Type = type;
            PlayerClassType = e.ParseUshort("PlayerClassType");
        }
    }

    public class ActivateEffectDesc
    {
        public readonly ActivateEffectIndex Index;
        public readonly ConditionEffectDesc[] Effects;
        public readonly ConditionEffectIndex Effect;
        public readonly string Id;
        public readonly int DurationMS;
        public readonly float Range;
        public readonly int Amount;
        public readonly int TotalDamage;
        public readonly float Radius;
        public readonly uint? Color;
        public readonly int MaxTargets;
        public readonly int Stat;

        public ActivateEffectDesc(XElement e)
        {
            Index = (ActivateEffectIndex)Enum.Parse(typeof(ActivateEffectIndex), e.Value.Replace(" ", ""));
            Id = e.ParseString("@id");
            Effect = e.ParseConditionEffect("@effect");
            DurationMS = (int)(e.ParseFloat("@duration") * 1000);
            Range = e.ParseFloat("@range");
            Amount = e.ParseInt("@amount");
            TotalDamage = e.ParseInt("@totalDamage");
            Radius = e.ParseFloat("@radius");
            MaxTargets = e.ParseInt("@maxTargets");
            Stat = e.ParseInt("@stat", -1);

            Effects = new ConditionEffectDesc[1]
            {
                new ConditionEffectDesc(Effect, DurationMS)
            };

            if (e.Attribute("color") != null)
                Color = e.ParseUInt("@color");
        }
    }

    public class ItemDesc
    {
        public readonly string Id;
        public readonly ushort Type;

        public readonly ItemType SlotType;
        public readonly int Tier;
        public readonly string Description;
        public readonly float RateOfFire;
        public readonly bool Usable;
        public readonly int BagType;
        public readonly int MpCost;
        public readonly int FameBonus;
        public readonly int NumProjectiles;
        public readonly float ArcGap;
        public readonly bool Consumable;
        public readonly bool Potion;
        public readonly string DisplayId;
        public readonly string SuccessorId;
        public readonly bool Soulbound;
        public readonly int CooldownMS;
        public readonly bool Resurrects;
        public readonly int Tex1;
        public readonly int Tex2;
        public readonly int Doses;

        public readonly KeyValuePair<int, int>[] StatBoosts;
        public readonly ActivateEffectDesc[] ActivateEffects;
        public readonly ProjectileDesc Projectile;

        public ItemDesc(XElement e, string id, ushort type)
        {
            Id = id;
            Type = type;

            SlotType = (ItemType)e.ParseInt("SlotType");
            Tier = e.ParseInt("Tier", -1);
            Description = e.ParseString("Description");
            RateOfFire = e.ParseFloat("RateOfFire", 1);
            Usable = e.ParseBool("Usable");
            BagType = e.ParseInt("BagType");
            MpCost = e.ParseInt("MpCost");
            FameBonus = e.ParseInt("FameBonus");
            NumProjectiles = e.ParseInt("NumProjectiles", 1);
            ArcGap = e.ParseFloat("ArcGap", 11.25f);
            Consumable = e.ParseBool("Consumable");
            Potion = e.ParseBool("Potion");
            DisplayId = e.ParseString("DisplayId", Id);
            Doses = e.ParseInt("Doses");
            SuccessorId = e.ParseString("SuccessorId");
            Soulbound = e.ParseBool("Soulbound");
            CooldownMS = (int)(e.ParseFloat("Cooldown", .2f) * 1000);
            Resurrects = e.ParseBool("Resurrects");
            Tex1 = (int)e.ParseUInt("Tex1");
            Tex2 = (int)e.ParseUInt("Tex2");

            var stats = new List<KeyValuePair<int, int>>();
            foreach (var s in e.Elements("ActivateOnEquip"))
                stats.Add(new KeyValuePair<int, int>(
                    s.ParseInt("@stat"),
                    s.ParseInt("@amount")));
            StatBoosts = stats.ToArray();

            var activate = new List<ActivateEffectDesc>();
            foreach (var i in e.Elements("Activate"))
                activate.Add(new ActivateEffectDesc(i));
            ActivateEffects = activate.ToArray();

            if (e.Element("Projectile") != null)
                Projectile = new ProjectileDesc(e.Element("Projectile"), Type);
        }
    }

    public class TileDesc
    {
        public readonly string Id;
        public readonly ushort Type;
        public readonly bool NoWalk;
        public readonly int Damage;
        public readonly float Speed;
        public readonly bool Sinking;
        public readonly bool Push;
        public readonly float DX;
        public readonly float DY;

        public readonly TextureData TextureData;
        public readonly int BlendPriority;
        public readonly int CompositePriority;
        public readonly bool HasEdge;
        public readonly TextureData EdgeTextureData;
        public readonly TextureData CornerTextureData;
        public readonly TextureData InnerCornerTextureData;
        public readonly bool SameTypeEdgeMode;

        private Sprite[] _edges;
        private Sprite[] _innerCorners;
        public TileDesc(XElement e, string id, ushort type) {
            Id = id;
            Type = type;
            NoWalk = e.ParseBool("NoWalk");
            Damage = e.ParseInt("Damage");
            Speed = e.ParseFloat("Speed", 1.0f);
            Sinking = e.ParseBool("Sinking");
            if (Push = e.ParseBool("Push"))
            {
                DX = e.Element("Animate").ParseFloat("@dx") / 1000f;
                DY = e.Element("Animate").ParseFloat("@dy") / 1000f;
            }
        }

        public Sprite[] GetEdges() {
            if (!HasEdge || _edges != null)
                return _edges;

            _edges = new Sprite[9];
            _edges[3] = EdgeTextureData.GetTexture();
            _edges[1] = SpriteUtils.Rotate(_edges[3], 3);
            _edges[5] = SpriteUtils.Rotate(_edges[3], 2);
            _edges[7] = SpriteUtils.Rotate(_edges[3], 1);
            if (CornerTextureData != null) {
                _edges[0] = SpriteUtils.CreateSingleTextureSprite(CornerTextureData.GetTexture());
                _edges[2] = SpriteUtils.Rotate(_edges[0], 1);
                _edges[8] = SpriteUtils.Rotate(_edges[0], 2);
                _edges[6] = SpriteUtils.Rotate(_edges[0], 3);
            }

            return _edges;
        }

        public Sprite[] GetInnerCorners() {
            if (InnerCornerTextureData == null || _innerCorners != null)
                return _innerCorners;

            _innerCorners = _edges.ToArray();
            _innerCorners[0] = SpriteUtils.CreateSingleTextureSprite(InnerCornerTextureData.GetTexture());
            _innerCorners[2] = SpriteUtils.Rotate(_innerCorners[0], 1);
            _innerCorners[8] = SpriteUtils.Rotate(_innerCorners[0], 2);
            _innerCorners[6] = SpriteUtils.Rotate(_innerCorners[0], 3);

            return _innerCorners;
        }
    }

    public class ProjectileDesc
    {
        public readonly byte BulletType;
        public readonly string ObjectId;
        public readonly int LifetimeMS;
        public readonly float Speed;

        public readonly int Damage;
        public readonly int MinDamage; //Only for players
        public readonly int MaxDamage;

        public readonly ConditionEffectDesc[] Effects;

        public readonly bool MultiHit;
        public readonly bool PassesCover;
        public readonly bool ArmorPiercing;
        public readonly bool Wavy;
        public readonly bool Parametric;
        public readonly bool Boomerang;

        public readonly float Amplitude;
        public readonly float Frequency;
        public readonly float Magnitude;

        public readonly bool Accelerate;
        public readonly bool Decelerate;

        public readonly ushort ContainerType;

        public ProjectileDesc(XElement e, ushort containerType)
        {
            ContainerType = containerType;
            BulletType = (byte)e.ParseInt("@id");
            ObjectId = e.ParseString("ObjectId");
            LifetimeMS = e.ParseInt("LifetimeMS");
            Speed = e.ParseFloat("Speed");
            Damage = e.ParseInt("Damage");
            MinDamage = e.ParseInt("MinDamage", Damage);
            MaxDamage = e.ParseInt("MaxDamage", Damage);

            var effects = new List<ConditionEffectDesc>();
            foreach (var k in e.Elements("ConditionEffect"))
                effects.Add(new ConditionEffectDesc(k));
            Effects = effects.ToArray();

            MultiHit = e.ParseBool("MultiHit");
            PassesCover = e.ParseBool("PassesCover");
            ArmorPiercing = e.ParseBool("ArmorPiercing");
            Wavy = e.ParseBool("Wavy");
            Parametric = e.ParseBool("Parametric");
            Boomerang = e.ParseBool("Boomerang");

            Amplitude = e.ParseFloat("Amplitude");
            Frequency = e.ParseFloat("Frequency", 1);
            Magnitude = e.ParseFloat("Magnitude", 3);

            Accelerate = e.ParseBool("Accelerate");
            Decelerate = e.ParseBool("Decelerate");
        }
    }

    public class ConditionEffectDesc
    {
        public readonly ConditionEffectIndex Effect;
        public readonly int DurationMS;

        public ConditionEffectDesc(ConditionEffectIndex effect, int durationMs)
        {
            Effect = effect;
            DurationMS = durationMs;
        }

        public ConditionEffectDesc(XElement e)
        {
            Effect = (ConditionEffectIndex)Enum.Parse(typeof(ConditionEffectIndex), e.Value.Replace(" ", ""));
            DurationMS = (int)(e.ParseFloat("@duration") * 1000);
        }
    }
}
