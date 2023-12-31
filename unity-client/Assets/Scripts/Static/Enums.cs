﻿using System;

namespace Static
{
    public enum Action
    {
        Stand,
        Walk,
        Attack
    }
    public enum Facing
    {
        Up,
        Down,
        Left,
        Right
    }
    public enum ModelType {
        None,
        Wall,
        Tower,
        Pillar,
        BrokenPillar,
        //GasEmitter,
        //Tower,
        //Web,
        //Cube,
        //BigCube,
        //Ico,
        //Octa,
        //Pyramid,
        //Tetra,
        //Dodec,
        //Pillar,
        //BrokenPillar,
        //Table,
        //Sign,
        //TableEdge,
        //Tesla,
        //CloningVat,
        //MosterTank1,
        //MosterTank2,
        //MosterTank3,
        //MosterTank4,
        //CandyColBroken,
        //CandyColWhole,
        //CandyDoughnut1,
        //CandyDoughnut2,
        //CandyDoughnut3,
    }
    public enum GameObjectType
    {
        Entity,
        Static,
        Interactive,
        Projectile,
        Player,
        Model,
    }
    public enum ObjectType
    {
        GameObject,
        Equipment,
        Skin,
        Dye,
        Character,
        CharacterChanger,
        ConnectedWall,
        Container,
        GuildChronicle,
        GuildHallPortal,
        GuildRegister,
        GuildMerchant,
        GuildBoard,
        MarketObject,
        Merchant,
        NameChanger,
        OneWayContainer,
        Player,
        Portal,
        Projectile,
        ReskinVendor,
        Stalagmite,
        VaultChest,
        ClosedVaultChest,
        ClosedGiftChest,
        MoneyChanger,
        SpiderWeb,
        PetUpgrader,
        CaveWall,
        Sign,
        Wall,
    }
    public enum StatType : byte
    {
        MaximumHP = 0,
        HP = 1,
        Size = 2,
        MaximumMP = 3,
        MP = 4,
        ExperienceGoal = 5,
        Experience = 6,
        Level = 7,
        Inventory0 = 8,
        Inventory1 = 9,
        Inventory2 = 10,
        Inventory3 = 11,
        Inventory4 = 12,
        Inventory5 = 13,
        Inventory6 = 14,
        Inventory7 = 15,
        Inventory8 = 16,
        Inventory9 = 17,
        Inventory10 = 18,
        Inventory11 = 19,
        Attack = 20,
        Defense = 21,
        Speed = 22,
        Vitality = 26,
        Wisdom = 27,
        Dexterity = 28,
        Effects = 29,
        Stars = 30,
        Name = 31,
        Texture1 = 32,
        Texture2 = 33,
        MerchantMerchandiseType = 34,
        Credits = 35,
        SellablePrice = 36,
        PortalUsable = 37,
        AccountId = 38,
        CurrentFame = 39,
        SellablePriceCurrency = 40,
        ObjectConnection = 41,
        MerchantRemainingCount = 42,
        MerchantRemainingMinute = 43,
        MerchantDiscount = 44,
        SellableRankRequirement = 45,
        HPBoost = 46,
        MPBoost = 47,
        AttackBonus = 48,
        DefenseBonus = 49,
        SpeedBonus = 50,
        VitalityBonus = 51,
        WisdomBonus = 52,
        DexterityBonus = 53,
        OwnerAccountId = 54,
        NameChangerStar = 55,
        NameChosen = 56,
        Fame = 57,
        FameGoal = 58,
        Glow = 59,
        SinkLevel = 60,
        AltTextureIndex = 61,
        GuildName = 62,
        GuildRank = 63,
        OxygenBar = 64,
        HealthStackCount = 65,
        MagicStackCount = 66,
        BackPack0 = 67,
        BackPack1 = 68,
        BackPack2 = 69,
        BackPack3 = 70,
        BackPack4 = 71,
        BackPack5 = 72,
        BackPack6 = 73,
        BackPack7 = 74,
        HasBackpack = 75,
        Skin = 76,

        None = 255
    }

    public enum Currency
    {
        Gold,
        Fame,
        GuildFame
    }

    public enum ItemData : ulong
    {
        //Tiers
        T0 = 1 << 0,
        T1 = 1 << 1,
        T2 = 1 << 2,
        T3 = 1 << 3,
        T4 = 1 << 4,
        T5 = 1 << 5,
        T6 = 1 << 6,
        T7 = 1 << 7,

        //Bonuses
        MaxHP = 1 << 8,
        MaxMP = 1 << 9,
        Attack = 1 << 10,
        Defense = 1 << 11,
        Speed = 1 << 12,
        Dexterity = 1 << 13,
        Vitality = 1 << 14,
        Wisdom = 1 << 15,
        RateOfFire = 1 << 16,
        Damage = 1 << 17,
        Cooldown = 1 << 18,
        FameBonus = 1 << 19
    }

    [Flags]
    public enum ConditionEffects : ulong
    {
        Nothing = 1 << 0,
        Quiet = 1 << 1,
        Weak = 1 << 2,
        Slowed = 1 << 3,
        Sick = 1 << 4,
        Dazed = 1 << 5,
        Stunned = 1 << 6,
        Blind = 1 << 7,
        Hallucinating = 1 << 8,
        Drunk = 1 << 9,
        Confused = 1 << 10,
        StunImmume = 1 << 11,
        Invisible = 1 << 12,
        Paralyzed = 1 << 13,
        Speedy = 1 << 14,
        Bleeding = 1 << 15,
        Healing = 1 << 16,
        Damaging = 1 << 17,
        Berserk = 1 << 18,
        Stasis = 1 << 19,
        StasisImmune = 1 << 20,
        Invincible = 1 << 21,
        Invulnerable = 1 << 23,
        Armored = 1 << 24,
        ArmorBroken = 1 << 25,
        Hexed = 1 << 26,
        NinjaSpeedy = 1 << 27,
    }

    public enum ConditionEffectIndex
    {
        Nothing = 0,
        Quiet = 1,
        Weak = 2,
        Slowed = 3,
        Sick = 4,
        Dazed = 5,
        Stunned = 6,
        Blind = 7,
        Hallucinating = 8,
        Drunk = 9,
        Confused = 10,
        StunImmune = 11,
        Invisible = 12,
        Paralyzed = 13,
        Speedy = 14,
        Bleeding = 15,
        Healing = 16,
        Damaging = 17,
        Berserk = 18,
        Stasis = 19,
        StasisImmune = 20,
        Invincible = 21,
        Invulnerable = 22,
        Armored = 23,
        ArmorBroken = 24,
        Hexed = 25,
    }

    public enum ActivateEffectIndex
    {
        Create,
        Dye,
        Shoot,
        IncrementStat,
        Heal,
        Magic,
        HealNova,
        StatBoostSelf,
        StatBoostAura,
        BulletNova,
        ConditionEffectSelf,
        ConditionEffectAura,
        Teleport,
        PoisonGrenade,
        VampireBlast,
        Trap,
        StasisBlast,
        Pet,
        Decoy,
        Lightning,
        UnlockPortal,
        MagicNova,
        ClearConditionEffectAura,
        RemoveNegativeConditions,
        ClearConditionEffectSelf,
        ClearConditionsEffectSelf,
        RemoveNegativeConditionsSelf,
        ShurikenAbility,
        DazeBlast,
        Backpack,
        PermaPet
    }

    public enum ShowEffectIndex
    {
        Unknown = 0,
        Heal = 1,
        Teleport = 2,
        Stream = 3,
        Throw = 4,
        Nova = 5,
        Poison = 6,
        Line = 7,
        Burst = 8,
        Flow = 9,
        Ring = 10,
        Lightning = 11,
        Collapse = 12,
        Coneblast = 13,
        Jitter = 14,
        Flash = 15,
        ThrowProjectile = 16
    }

    public enum ItemType : byte
    {
        All,
        Sword,
        Dagger,
        Bow,
        Tome,
        Shield,
        Leather,
        Plate,
        Wand,
        Ring,
        Potion,
        Spell,
        Seal,
        Cloak,
        Robe,
        Quiver,
        Helm,
        Staff,
        Poison,
        Skull,
        Trap,
        Orb,
        Prism,
        Scepter,
        Katana,
        Shuriken,
    }
}
