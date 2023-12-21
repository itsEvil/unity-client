using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using Networking.Tcp;
using System.Text;

namespace Static {
    public struct TextureSheetData {
        public SpriteSheetData[] SSD;
        public Texture2D[] Textures;
        public TextureSheetData(SpriteSheetData[] ssd, Texture2D[] tex) {
            SSD = ssd;
            Textures = tex;
        }
    }
    public readonly struct SpriteSheetData {
        public readonly string Id;
        public readonly string SheetName;
        public readonly int AnimationWidth;
        public readonly int AnimationHeight;
        public readonly Facing StartFacing;
        public readonly int ImageWidth;
        public readonly int ImageHeight;

        public SpriteSheetData(XElement xml) {
            Id = xml.ParseString("@id");
            SheetName = xml.ParseString("@sheetName", Id);

            var animationSize = xml.ParseIntArray("AnimationSize", "x", new[] { 0, 0 });
            AnimationWidth = animationSize[0];
            AnimationHeight = animationSize[1];
            StartFacing = xml.ParseEnum("StartDirection", Facing.Right);

            var imageSize = xml.ParseIntArray("ImageSize", "x", new[] { 0, 0 });
            ImageWidth = imageSize[0];
            ImageHeight = imageSize[1];
        }

        public bool IsAnimation() {
            return AnimationWidth != 0 && AnimationHeight != 0;
        }
    }
    public readonly struct GameInitData
    {
        public readonly int WorldId;
        public readonly short CharId;
        public readonly bool NewCharacter;
        public readonly short ClassType;
        public readonly short SkinType;
        public GameInitData(int worldId, short charId, bool newCharacter, short classType, short skinType)
        {
            WorldId = worldId;
            CharId = charId;
            NewCharacter = newCharacter;
            ClassType = classType;
            SkinType = skinType;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Vec2 {
        [FieldOffset(0)]
        public float x;
        [FieldOffset(4)]
        public float y;

        public readonly static Vec2 zero = new(0f, 0f);
        public readonly static Vec2 one = new(1f, 1f);
        public readonly float Length => (float)Math.Sqrt(x * x + y * y);
        public readonly float SqrLength => x * x + y * y;
        public readonly float Angle => (float)Math.Atan2(y, x);

        public Vec2(float x, float y) {
            this.x = x;
            this.y = y;
        }

        //public Vec2 Clamp(float min, float max)
        //{
        //    return Clamp(new Vec2(min, min), new Vec2(max, max));
        //}

        public readonly Vec2 Add(Vec2 vec)
            => new(x + vec.x, y + vec.y);
        public readonly Vec2 Add(Vector2 vec)
            => new(x + vec.x, y + vec.y);
        public readonly Vec2 Add(Vector3 vec)
            => new(x + vec.x, y + vec.y);
        public readonly Vec2 Subtract(Vec2 vec)
            => new(x - vec.x, y - vec.y);
        public readonly Vec2 Subtract(Vector2 vec)
            => new(x - vec.x, y - vec.y);
        public readonly Vec2 Subtract(Vector3 vec)
            => new(x - vec.x, y - vec.y);
        public readonly Vec2 Multiply(Vec2 vec)
            => new(x * vec.x, y * vec.y);
        public readonly Vec2 Multiply(Vector2 vec)
            => new(x * vec.x, y * vec.y);
        public readonly Vec2 Multiply(Vector3 vec)
            => new(x * vec.x, y * vec.y);
        public readonly Vec2 Divide(Vec2 vec)
            => new(x / vec.x, y / vec.y);
        public readonly Vec2 Divide(Vector2 vec)
            => new(x / vec.x, y / vec.y);
        public readonly Vec2 Divide(Vector3 vec)
            => new(x / vec.x, y / vec.y);
        public readonly Vec2 Add(float value)
            => new(x + value, y + value);
        public readonly Vec2 Subtract(float value)
            => new(x - value, y - value);
        public readonly Vec2 Multiply(float value)
            => new(x * value, y * value);
        public readonly Vec2 Divide(float value)
            => new(x / value, y / value);
        public readonly Vec2 RotateAround(Vec2 pivot, float radians)
            => Subtract(pivot).RotateOrigin(radians).Add(pivot);
        public readonly Vec2 RotateAround(Vec2 pivot, float sin, float cos)
            => Subtract(pivot).RotateOrigin(sin, cos).Add(pivot);
        public readonly Vec2 RotateOrigin(float radians)
            => RotateOrigin((float)Math.Sin(radians), (float)Math.Cos(radians));
        public readonly Vec2 RotateOrigin(float sin, float cos)
            => new(x * cos - y * sin, x * sin + y * cos);
        public readonly float AngleTo(Vec2 vec)
            => (float)Math.Atan2(vec.y - y, vec.x - x);
        public readonly float DistanceTo(Vec2 vec)
            => vec.Subtract(this).Length;
        public readonly float SqrDistanceTo(Vec2 vec)
            => vec.Subtract(this).SqrLength;
        public readonly Vec2 Normalize() {
            float length = Length;
            return new Vec2(x / length, y / length);
        }
        public readonly Vec2 Invert() => this * -1f;
        public readonly Vec2 ChangeLength(float length) {
            float length2 = Length;
            return new Vec2(x / length2 * length, y / length2 * length);
        }
        public readonly Vec2 ChangeLength(float length, float currentLength)
            => new(x / currentLength * length, y / currentLength * length);
        public readonly bool LongerThan(float radius)
            => SqrLength > radius * radius;
        public readonly bool RadiusContains(Vec2 position, float radius)
            => !(position - this).LongerThan(radius);
        public static Vec2 FromAngle(float radians)
            => new((float)Math.Cos(radians), (float)Math.Sin(radians));
        public static Vec2 FromAngle(float sin, float cos)
            => new(cos, sin);
        public static Vec2 operator +(Vec2 a, Vec2 b)
            => a.Add(b);
        public static Vec2 operator -(Vec2 a, Vec2 b)
            => a.Subtract(b);
        public static Vec2 operator *(Vec2 a, Vec2 b)
            => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, Vec2 b)
            => a.Divide(b);
        public static Vec2 operator +(Vec2 a, float b)
            => a.Add(b);
        public static Vec2 operator -(Vec2 a, float b)
            => a.Subtract(b);
        public static Vec2 operator *(Vec2 a, float b)
            => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, float b)
            => a.Divide(b);
        public static bool operator ==(Vec2 a, Vec2 b) {
            if (a.x == b.x) 
                return a.y == b.y;
            
            return false;
        }
        public static bool operator !=(Vec2 a, Vec2 b) {
            if (a.x == b.x) 
                return a.y != b.y;
            
            return true;
        }

        public static implicit operator Vec2(float value)
            => new(value, value);
        public static Vec2 operator +(Vec2 a, Vector3 b)
            => a.Add(b);
        public static Vec2 operator *(Vec2 a, Vector3 b)
            => a.Multiply(b);
        public static Vec2 operator /(Vec2 a, Vector3 b)
            => a.Divide(b);
        public static Vec2 operator -(Vec2 a) 
            => new(0f - a.x, 0f - a.y);
        
        public readonly override string ToString()
            => $"{x}, {y}";
        public readonly override bool Equals(object obj) {
            if (obj is Vec2 vec) 
                return this == vec;
            return base.Equals(obj);
        }
        public readonly override int GetHashCode()
            => base.GetHashCode();

        public readonly Vector2 ToVector2() => new Vector2(x,y);
    }

    public readonly struct ClassStats
    {
        public readonly int ClassType;
        public readonly int BestLevel;
        public readonly int BestFame;

        public ClassStats(int classType, int bestLevel, int bestFame)
        {
            ClassType = classType;
            BestLevel = bestLevel;
            BestFame = bestFame;
        }

        public ClassStats(XElement xml)
        {
            ClassType = xml.ParseInt("@objectType");
            BestLevel = xml.ParseInt("BestLevel");
            BestFame = xml.ParseInt("BestFame");
        }
    }
    public readonly struct NewsInfos
    {
        public readonly string Icon;
        public readonly string Title;
        public readonly string TagLine;
        public readonly string Link;
        public readonly int Date;

        public NewsInfos(XElement xml)
        {
            Icon = xml.ParseString("Icon");
            Title = xml.ParseString("Title");
            TagLine = xml.ParseString("TagLine");
            Link = xml.ParseString("Link");
            Date = xml.ParseInt("Date");
        }
        public override string ToString()
        {
            return $"[News] [Icon:{Icon}] [Title:{Title}]";
        }
    }
    public readonly struct DeathCharStats
    {
        public readonly int Id;
        public readonly string AccountName;
        public readonly int ObjectType;
        public readonly int Level;
        public readonly int Exp;
        public readonly int MaxHitPoints;
        public readonly int HitPoints;
        public readonly int MaxMagicPoints;
        public readonly int MagicPoints;
        public readonly int Strength;
        public readonly int Dexterity;
        public readonly int Intelligence;
        public readonly int Vitality;
        public readonly int Armor;
        public readonly int Evasion;
        public readonly int CriticalDamage;
        public readonly int CriticalChance;
        public DeathCharStats(XElement data)
        {
            Id = data.ParseInt("@id");
            ObjectType = data.ParseInt("ObjectType");
            Level = data.ParseInt("Level");
            Exp = data.ParseInt("Exp");
            AccountName = data.Element("Account").ParseString("Name");
            MaxHitPoints = data.ParseInt("MaxHitPoints");
            HitPoints = data.ParseInt("HitPoints");
            MaxMagicPoints = data.ParseInt("MaxMagicPoints");
            MagicPoints = data.ParseInt("MagicPoints");
            Strength = data.ParseInt("Strength");
            Dexterity = data.ParseInt("Dexterity");
            Intelligence = data.ParseInt("Intelligence");
            Vitality = data.ParseInt("Vitality");
            Armor = data.ParseInt("Armor");
            Evasion = data.ParseInt("Evasion");
            CriticalChance = data.ParseInt("CriticalChance");
            CriticalDamage = data.ParseInt("CriticalDamage");

        }
    }
    public readonly struct DeathBonus
    {
        public readonly string Id;
        public readonly int Amount;
        public DeathBonus(XElement data)
        {
            Id = data.ParseString("@id");
            Amount = int.Parse(data.Value);
        }
    }
    public readonly struct DeathStats
    {
        public readonly int Shots;
        public readonly int ShotsThatDamage;
        public readonly int TilesUncovered;
        public readonly int QuestsCompleted;
        public readonly int PirateCavesCompleted;
        public readonly int UndeadLairsCompleted;
        public readonly int AbyssOfDemonsCompleted;
        public readonly int SnakePitsCompleted;
        public readonly int SpiderDensCompleted;
        public readonly int SpriteWorldsCompleted;
        public readonly int Escapes;
        public readonly int NearDeathEscapes;
        public readonly int LevelUpAssists;
        public readonly int DamageTaken;
        public readonly int DamageDealt;
        public readonly int Teleports;
        public readonly int PotionsDrank;
        public readonly int MonsterKills;
        public readonly int MonsterAssists;
        public readonly int GodKills;
        public readonly int GodAssists;
        public readonly int OryxKills;
        public readonly int OryxAssists;
        public readonly int CubeKills;
        public readonly int CubeAssists;
        public readonly int LGBags;
        public readonly int GLBags;
        public readonly int ATBags;
        public readonly int MinutesActive;
        public readonly int AbilitiesUsed;

        public DeathStats(XElement data)
        {
            Shots = data.ParseInt("Shots");
            ShotsThatDamage = data.ParseInt("ShotsThatDamage");
            TilesUncovered = data.ParseInt("TilesUncovered");
            QuestsCompleted = data.ParseInt("QuestsCompleted");
            PirateCavesCompleted = data.ParseInt("PirateCavesCompleted");
            UndeadLairsCompleted = data.ParseInt("UndeadLairsCompleted");
            AbyssOfDemonsCompleted = data.ParseInt("AbyssOfDemonsCompleted");
            SnakePitsCompleted = data.ParseInt("SnakePitsCompleted");
            SpiderDensCompleted = data.ParseInt("SpiderDensCompleted");
            SpriteWorldsCompleted = data.ParseInt("SpriteWorldsCompleted");
            Escapes = data.ParseInt("Escapes");
            NearDeathEscapes = data.ParseInt("NearDeathEscapes");
            LevelUpAssists = data.ParseInt("LevelUpAssists");
            DamageTaken = data.ParseInt("DamageTaken");
            DamageDealt = data.ParseInt("DamageDealt");
            Teleports = data.ParseInt("Teleports");
            PotionsDrank = data.ParseInt("PotionsDrank");
            MonsterKills = data.ParseInt("MonsterKills");
            MonsterAssists = data.ParseInt("MonsterAssists");
            GodKills = data.ParseInt("GodKills");
            GodAssists = data.ParseInt("GodAssists");
            OryxAssists = data.ParseInt("OryxAssists");
            OryxKills = data.ParseInt("OryxKills");
            CubeKills = data.ParseInt("CubeKills");
            CubeAssists = data.ParseInt("CubeAssists");
            LGBags = data.ParseInt("LGBags");
            GLBags = data.ParseInt("GLBags");
            ATBags = data.ParseInt("ATBags");
            MinutesActive = data.ParseInt("MinutesActive");
            AbilitiesUsed = data.ParseInt("AbilitiesUsed");
        }
    }
    public readonly struct CharacterStats
    {
        public readonly int Id;
        public readonly int ClassType;
        public readonly int Level;
        public readonly int Experience;
        public readonly int Fame;
        public readonly int[] Stats;
        public readonly ushort[] Inventory;
        public readonly int HP;
        public readonly int MP;
        public readonly int Tex1;
        public readonly int Tex2;
        public readonly int SkinType;
        public readonly bool Dead;
        public readonly bool Hasbackpack;
        public readonly int Size;
        public readonly uint Glow;

        public CharacterStats(XElement xml)
        {
            Id = xml.ParseInt("@id");
            ClassType = xml.ParseInt("ObjectType");
            Level = xml.ParseInt("Level");
            Experience = xml.ParseInt("Exp");
            HP = xml.ParseInt("HitPoints");
            MP = xml.ParseInt("MagicPoints");
            Stats = xml.ParseIntArray("Stats", ",", Array.Empty<int>());
            Inventory= xml.ParseUshortArray("Inventory", ",", Array.Empty<ushort>());
            Fame = xml.ParseInt("CurrentFame");
            Tex1 = xml.ParseInt("Tex1");
            Tex2 = xml.ParseInt("Tex2");
            SkinType = xml.ParseInt("SkinType");
            Dead = xml.ParseBool("Dead");
            Hasbackpack = xml.ParseBool("Backpack");
            Glow = xml.ParseUInt("Glow");
            Size = xml.ParseInt("Size");
        }
        public override string ToString()
        {
            return $"[Char Stats] [Id:{Id}] [ClassType:{ClassType}]";
        }

        public string ToLongString()
        {
            return $"[Char Stats] [Id:{Id}] [ClassType:{ClassType}] " +
                $"[Level:{Level}] [XP:{Experience}] [HP:{HP}] [MP:{MP}] [Stats:{string.Join(",", Stats)}] " +
                $"[Inventory:{string.Join(",", Inventory.ToString())}]";
        }
        public int[] GetStatArray() {
            return Stats;
        }
    }
    public readonly struct FameStats
    {
        public readonly int Id;
        public readonly int ClassType;
        public readonly int Level;
        public readonly int Experience;
        public readonly int Fame;
        public readonly int[] Stats;
        public readonly ushort[] Inventory;
        public readonly int HP;
        public readonly int MP;
        public readonly int Tex1;
        public readonly int Tex2;
        public readonly int SkinType;
        public readonly string AccountName;
        public readonly int Shots;
        public readonly int ShotsThatDamage;
        public readonly int TilesUncovered;
        public readonly int QuestsCompleted;
        public readonly int PirateCavesCompleted;
        public readonly int UndeadLairsCompleted;
        public readonly int AbyssOfDemonsCompleted;
        public readonly int SnakePitsCompleted;
        public readonly int SpiderDensCompleted;
        public readonly int SpriteWorldsCompleted;
        public readonly int Escapes;
        public readonly int NearDeathEscapes;
        public readonly int LevelUpAssits;
        public readonly int DamageTaken;
        public readonly int DamageDealt;
        public readonly int Teleports;
        public readonly int PotionsDrunk;
        public readonly int MosterKills;
        public readonly int MonsterAssists;
        public readonly int GodKills;
        public readonly int GodAssists;
        public readonly int OryxKills;
        public readonly int OryxAssists;
        public readonly int CubeKills;
        public readonly int CubeAssists;
        public readonly int MinutesActive;
        public readonly int AbilitiesUsed;
        public readonly TimeSpan CreatedOn;
        public readonly TimeSpan KilledOn;
        public readonly string KilledBy;
        public readonly int BaseFame;
        public readonly int TotalFame;
        public FameStats(XElement data)
        {
            XElement xml = data.Element("Fame").Element("Char");

            Id = xml.ParseInt("@id");
            ClassType = xml.ParseInt("ObjectType");
            Level = xml.ParseInt("Level");
            Experience = xml.ParseInt("Exp");
            HP = xml.ParseInt("HitPoints");
            MP = xml.ParseInt("MagicPoints");
            Stats = xml.ParseIntArray("Stats", ",", Array.Empty<int>());
            Inventory = xml.ParseUshortArray("Inventory", ",", Array.Empty<ushort>());
            Fame = xml.ParseInt("CurrentFame");
            Tex1 = xml.ParseInt("Tex1");
            Tex2 = xml.ParseInt("Tex2");
            SkinType = xml.ParseInt("SkinType");
            Shots = xml.ParseInt("Shots");

            if (xml.Element("Account") != null)
            {
                var account = xml.Element("Account");
                AccountName = account.ParseString("Name");
            }
            else AccountName = "Unknown";

            AbilitiesUsed = xml.ParseInt("AbilitiesUsed");
            ShotsThatDamage = xml.ParseInt("ShotsThatDamage");
            TilesUncovered = xml.ParseInt("TilesUncovered");
            QuestsCompleted = xml.ParseInt("QuestsCompleted");
            PirateCavesCompleted = xml.ParseInt("PiratesCavesCompleted");
            UndeadLairsCompleted = xml.ParseInt("UndeadLairsCompleted");
            AbyssOfDemonsCompleted = xml.ParseInt("AbyssOfDemonsCompleted");
            SnakePitsCompleted = xml.ParseInt("SnakePitsCompleted");
            SpiderDensCompleted = xml.ParseInt("SpiderDensCompleted");
            SpriteWorldsCompleted = xml.ParseInt("SpriteWorldsCompleted");
            Escapes = xml.ParseInt("Escapes");
            NearDeathEscapes = xml.ParseInt("NearDeathEscapes");
            LevelUpAssits = xml.ParseInt("LevelUpAssists");
            DamageTaken = xml.ParseInt("DamageTaken");
            DamageDealt = xml.ParseInt("DamageDealt");
            Teleports = xml.ParseInt("Teleports");
            PotionsDrunk = xml.ParseInt("PotionsDrank");
            MosterKills = xml.ParseInt("MosterKills");
            MonsterAssists = xml.ParseInt("MonsterAssists");
            GodKills = xml.ParseInt("GodKills");
            GodAssists = xml.ParseInt("GodAssists");
            OryxKills = xml.ParseInt("OryxKills");
            OryxAssists = xml.ParseInt("OryxAssists");
            CubeKills = xml.ParseInt("CubeKills");
            CubeAssists = xml.ParseInt("CubeAssists");
            MinutesActive = xml.ParseInt("MinutesActive");
            CreatedOn = TimeSpan.FromSeconds(xml.ParseInt("CreatedOn"));
            KilledOn = TimeSpan.FromSeconds(xml.ParseUInt("KilledOn"));
            KilledBy = xml.ParseString("KilledBy");
            BaseFame = xml.ParseInt("BaseFame");
            TotalFame = xml.ParseInt("TotalFame");
        }
    }
    public readonly struct TileData {
        public readonly short X;
        public readonly short Y;
        public readonly ushort TileType;
        public TileData(Span<byte> buffer, ref int ptr, int len) {
            X = PacketUtils.ReadShort(buffer, ref ptr, len);
            Y = PacketUtils.ReadShort(buffer, ref ptr, len);
            TileType = PacketUtils.ReadUShort(buffer, ref ptr, len);
        }
    }
    public struct MoveRecord
    {
        public long Time;
        public float X;
        public float Y;
        public MoveRecord(long time, float x, float y) {
            Time = time;
            X = x;
            Y = y;
        }
        public readonly void Write(PacketWriter wtr) {
            wtr.Write(Time);
            wtr.Write(X);
            wtr.Write(Y);
        }
    }
    public readonly struct ObjectDrop {
        public readonly int Id;
        //public readonly bool Explode;
        public ObjectDrop(Span<byte> buffer, ref int ptr, int len) {
            Id = PacketUtils.ReadInt(buffer, ref ptr, len);
            //Explode = false;
            //Explode = PacketUtils.ReadBool(buffer, ref ptr, len);
        }
    }

    public readonly struct ObjectDefinition {
        public readonly ushort ObjectType;
        public readonly ObjectStatus ObjectStatus;
        public ObjectDefinition(Span<byte> buffer, ref int ptr, int len) {
            ObjectType = PacketUtils.ReadUShort(buffer, ref ptr, len);
            ObjectStatus = new ObjectStatus(buffer, ref ptr, len);
        }
    }
    public struct SlotObjectData
    {
        public int ObjectId;
        public int SlotId;
    }
    public readonly struct ObjectStatus {
        public readonly int Id;
        public readonly Vec2 Position;
        public readonly Dictionary<StatType, object> Stats;
        public ObjectStatus(Span<byte> buffer, ref int ptr, int len) {
            Id = PacketUtils.ReadInt(buffer, ref ptr, len);
            Position = new Vec2() {
                x = PacketUtils.ReadFloat(buffer, ref ptr, len),
                y = PacketUtils.ReadFloat(buffer, ref ptr, len),
            };

            var statsCount = PacketUtils.ReadUShort(buffer, ref ptr, len);
            var statsByteLength = PacketUtils.ReadUShort(buffer, ref ptr, len);
            var nextObjIdx = ptr + statsByteLength;
            Stats = new Dictionary<StatType, object>();
            for (var i = 0; i < statsCount; i++) {
                StatType key = (StatType)PacketUtils.ReadByte(buffer[ptr..], ref ptr, len);
                //Utils.Log("Reading Stat {0} {1} {2}", key, statsCount, i);
                if (!Enum.IsDefined(typeof(StatType), key)) {
                    Utils.Error("Key not defined in enum {0}", key);
                    ptr = nextObjIdx;
                    continue;
                }

                if (!ReadStat(key, buffer, ref ptr, len, out var data)) {
                    Utils.Error("Stat data parsing for stat {0} failed, object: {1}", key, Id);
                    ptr = nextObjIdx;
                    continue;
                }

                Stats[key] = data;
            }
            //var newPtr = PacketUtils.ReadUShort(buffer, ref ptr, len);

            //Debug
            //StringBuilder sb = new();
            //sb.Append($"Id: {Id} Position:[{Position.x}:{Position.y}] Stats {statsCount}:[");
            //foreach(var (key, value) in Stats)
            //    sb.Append($"|Key:{key} Value:{value}|");
            //
            //sb.Append($"]");
            //Utils.Log("Object Status: {0}", sb.ToString());
        }

        private static bool ReadStat(StatType key, Span<byte> buffer, ref int ptr, int len, out object data) {
            try {
                data = ParseStatData(key, buffer, ref ptr, len);
                return true;
            } catch(Exception e) {
                data = false;
                Utils.Error("Parsing stat failed: {0}::{1}", e, e.StackTrace);
                return false;
            }
        }

        private static object ParseStatData(StatType key, Span<byte> buffer, ref int ptr, int len) {
            return key switch
            {
                StatType.MaxHp or StatType.Hp or StatType.Size or StatType.MaxMp or StatType.Mp
                or StatType.NextLevelExp or StatType.Exp or StatType.Level or StatType.Inventory0
                or StatType.Inventory1 or StatType.Inventory2 or StatType.Inventory3 or StatType.Inventory4
                or StatType.Inventory5 or StatType.Inventory6 or StatType.Inventory7 or StatType.Inventory8
                or StatType.Inventory9 or StatType.Inventory10 or StatType.Inventory11 or StatType.Attack
                or StatType.Defense or StatType.Speed or StatType.Vitality or StatType.Wisdom or StatType.Dexterity
                or StatType.Backpack0 or StatType.Backpack1 or StatType.Backpack2 or StatType.Backpack3
                or StatType.Backpack4 or StatType.Backpack5 or StatType.Backpack6 or StatType.Backpack7
                or StatType.GuildRank or StatType.Breath or StatType.HealthPotionStack or StatType.MagicPotionStack
                or StatType.NumStars or StatType.Tex1 or StatType.Tex2 or StatType.MerchandiseCount
                or StatType.MerchandiseMinsLeft or StatType.MerchandiseRankReq or StatType.MerchandisePrice
                or StatType.Credits or StatType.AccountId or StatType.Fame or StatType.MaxHpBoost or StatType.MaxMpBoost
                or StatType.AttackBoost or StatType.DefenseBoost or StatType.SpeedBoost or StatType.VitalityBoost
                or StatType.WisdomBoost or StatType.DexterityBoost or StatType.CharFame or StatType.NextClassQuestFame
                    => PacketUtils.ReadInt(buffer, ref ptr, len),

                StatType.Condition => PacketUtils.ReadULong(buffer, ref ptr, len),
                StatType.Name => PacketUtils.ReadString(buffer, ref ptr, len),
                StatType.MerchandiseType => PacketUtils.ReadUShort(buffer, ref ptr, len),
                StatType.Active or StatType.HasBackpack or StatType.NameChosen => PacketUtils.ReadBool(buffer, ref ptr, len),
                StatType.MerchandiseDiscount or StatType.MerchandiseCurrency => PacketUtils.ReadByte(buffer[ptr..], ref ptr, len),
                _ => false,
            };
        }
    }
}
