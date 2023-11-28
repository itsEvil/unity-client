using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using Networking.Tcp;
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
        public readonly int CharId;
        public readonly bool NewCharacter;
        public readonly int ClassType;
        public readonly int SkinType;
        public readonly int Difficulty;

        public GameInitData(int worldId, int charId, bool newCharacter, int classType, int skinType, int difficulty)
        {
            WorldId = worldId;
            CharId = charId;
            NewCharacter = newCharacter;
            ClassType = classType;
            SkinType = skinType;
            Difficulty = difficulty;
        }
    }
    [StructLayout(LayoutKind.Explicit)]
    public struct Vec2
    {
        [FieldOffset(0)]
        public float x;

        [FieldOffset(4)]
        public float y;

        public readonly static Vec2 zero = new(0f, 0f);

        public readonly static Vec2 one = new(1f, 1f);

        public readonly float Length => (float)Math.Sqrt(x * x + y * y);

        public readonly float SqrLength => x * x + y * y;

        public readonly float Angle => (float)Math.Atan2(y, x);

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        //public Vec2 Clamp(float min, float max)
        //{
        //    return Clamp(new Vec2(min, min), new Vec2(max, max));
        //}

        public readonly Vec2 Add(Vec2 vec)
        {
            return new Vec2(x + vec.x, y + vec.y);
        }

        public readonly Vec2 Subtract(Vec2 vec)
        {
            return new Vec2(x - vec.x, y - vec.y);
        }

        public readonly Vec2 Multiply(Vec2 vec)
        {
            return new Vec2(x * vec.x, y * vec.y);
        }

        public readonly Vec2 Divide(Vec2 vec)
        {
            return new Vec2(x / vec.x, y / vec.y);
        }

        public readonly Vec2 Add(float value)
        {
            return new Vec2(x + value, y + value);
        }

        public readonly Vec2 Subtract(float value)
        {
            return new Vec2(x - value, y - value);
        }

        public readonly Vec2 Multiply(float value)
        {
            return new Vec2(x * value, y * value);
        }

        public readonly Vec2 Divide(float value)
        {
            return new Vec2(x / value, y / value);
        }

        public readonly Vec2 RotateAround(Vec2 pivot, float radians)
        {
            return Subtract(pivot).RotateOrigin(radians).Add(pivot);
        }

        public readonly Vec2 RotateAround(Vec2 pivot, float sin, float cos)
        {
            return Subtract(pivot).RotateOrigin(sin, cos).Add(pivot);
        }

        public readonly Vec2 RotateOrigin(float radians)
        {
            return RotateOrigin((float)Math.Sin(radians), (float)Math.Cos(radians));
        }

        public readonly Vec2 RotateOrigin(float sin, float cos)
        {
            return new Vec2(x * cos - y * sin, x * sin + y * cos);
        }

        public readonly float AngleTo(Vec2 vec)
        {
            return (float)Math.Atan2(vec.y - y, vec.x - x);
        }

        public readonly float DistanceTo(Vec2 vec)
        {
            return vec.Subtract(this).Length;
        }

        public readonly float SqrDistanceTo(Vec2 vec)
        {
            return vec.Subtract(this).SqrLength;
        }

        public readonly Vec2 Normalize()
        {
            float length = Length;
            return new Vec2(x / length, y / length);
        }

        public readonly Vec2 Invert()
        {
            return this * -1f;
        }

        public readonly Vec2 ChangeLength(float length)
        {
            float length2 = Length;
            return new Vec2(x / length2 * length, y / length2 * length);
        }

        public readonly Vec2 ChangeLength(float length, float currentLength)
        {
            return new Vec2(x / currentLength * length, y / currentLength * length);
        }

        public readonly bool LongerThan(float radius)
        {
            return SqrLength > radius * radius;
        }

        public readonly bool RadiusContains(Vec2 position, float radius)
        {
            return !(position - this).LongerThan(radius);
        }
        public static Vec2 FromAngle(float radians)
        {
            return new Vec2((float)Math.Cos(radians), (float)Math.Sin(radians));
        }

        public static Vec2 FromAngle(float sin, float cos)
        {
            return new Vec2(cos, sin);
        }

        public static Vec2 operator +(Vec2 a, Vec2 b)
        {
            return a.Add(b);
        }

        public static Vec2 operator -(Vec2 a, Vec2 b)
        {
            return a.Subtract(b);
        }

        public static Vec2 operator *(Vec2 a, Vec2 b)
        {
            return a.Multiply(b);
        }

        public static Vec2 operator /(Vec2 a, Vec2 b)
        {
            return a.Divide(b);
        }

        public static Vec2 operator +(Vec2 a, float b)
        {
            return a.Add(b);
        }

        public static Vec2 operator -(Vec2 a, float b)
        {
            return a.Subtract(b);
        }

        public static Vec2 operator *(Vec2 a, float b)
        {
            return a.Multiply(b);
        }

        public static Vec2 operator /(Vec2 a, float b)
        {
            return a.Divide(b);
        }

        public static bool operator ==(Vec2 a, Vec2 b)
        {
            if (a.x == b.x)
            {
                return a.y == b.y;
            }

            return false;
        }

        public static bool operator !=(Vec2 a, Vec2 b)
        {
            if (a.x == b.x)
            {
                return a.y != b.y;
            }

            return true;
        }

        public static implicit operator Vec2(float value)
        {
            return new Vec2(value, value);
        }

        public readonly override string ToString()
        {
            return $"{x}, {y}";
        }

        public readonly override bool Equals(object obj)
        {
            if (obj is Vec2 vec) {
                return this == vec;
            }

            return base.Equals(obj);
        }

        public readonly override int GetHashCode()
        {
            return base.GetHashCode();
        }
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
    public struct TileData
    {
        public ushort TileType;
        public short X;
        public short Y;

        public TileData(PacketReader rdr)
        {
            X = rdr.ReadInt16();
            Y = rdr.ReadInt16();
            TileType = rdr.ReadUInt16();
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
    public struct ObjectDrop
    {
        public int Id;
        public bool Explode;

        public ObjectDrop(PacketReader rdr)
        {
            Id = rdr.ReadInt32();
            Explode = rdr.ReadBoolean();
        }
    }

    public struct ObjectDefinition
    {
        public int ObjectType;
        public ObjectStatus ObjectStatus;

        public ObjectDefinition(PacketReader rdr)
        {
            ObjectType = rdr.ReadInt32();
            ObjectStatus = new ObjectStatus(rdr);
        }
    }
    public struct SlotObjectData
    {
        public int ObjectId;
        public int SlotId;
    }
    public struct ObjectStatus
    {
        public int Id;
        public Vector3 Position;
        public Dictionary<StatType, object> Stats;

        public ObjectStatus(PacketReader rdr)
        {
            Id = rdr.ReadInt32();
            Position = new Vector3()
            {
                x = rdr.ReadSingle(),
                y = rdr.ReadSingle(),
                z = 0
            };

            var statsCount = rdr.ReadByte();
            Stats = new Dictionary<StatType, object>();
            for (var i = 0; i < statsCount; i++) {
                var key = (StatType)rdr.ReadByte();
                if (IsStringStat(key)) {
                    Stats[key] = rdr.ReadString();
                }
                else {
                    Stats[key] = rdr.ReadInt32();
                }
            }
        }

        public static bool IsStringStat(StatType stat)
        {
            return stat switch
            {
                StatType.Name or StatType.GuildName => true,
                _ => false,
            };
        }
    }
}
