using Game;
using Static;
using System;
using UnityEngine;

namespace Networking.Tcp
{
    public enum C2SPacketId : byte
    {
        Unknown = 0,
        AcceptTrade = 1,
        AoeAck = 2,
        Buy = 3,
        CancelTrade = 4,
        ChangeGuildRank = 5,
        ChangeTrade = 6,
        ChooseName = 8,
        CreateGuild = 10,
        EditAccountList = 11,
        EnemyHit = 12,
        Escape = 13,
        GroundDamage = 15,
        GuildInvite = 16,
        GuildRemove = 17,
        Hello = 18,
        InvDrop = 19,
        InvSwap = 20,
        JoinGuild = 21,
        Move = 23,
        OtherHit = 24,
        PlayerHit = 25,
        PlayerShoot = 26,
        PlayerText = 27,
        Pong = 28,
        RequestTrade = 29,
        Reskin = 30,
        ShootAck = 32,
        SquareHit = 33,
        Teleport = 34,
        UpdateAck = 35,
        UseItem = 36,
        UsePortal = 37
    }
    public enum S2CPacketId : byte
    {
        Unknown = 0,
        AccountList = 1,
        AllyShoot = 2,
        Aoe = 3,
        BuyResult = 4,
        ClientStat = 5,
        CreateSuccess = 6,
        Damage = 7,
        Death = 8,
        EnemyShoot = 9,
        Failure = 10,
        File = 11,
        GlobalNotification = 12,
        Goto = 13,
        GuildResult = 14,
        InvResult = 15,
        InvitedToGuild = 16,
        MapInfo = 17,
        NameResult = 18,
        NewTick = 19,
        Notification = 20,
        Pic = 21,
        Ping = 22,
        PlaySound = 23,
        QuestObjId = 24,
        Reconnect = 25,
        ServerPlayerShoot = 26,
        ShowEffect = 27,
        Text = 28,
        TradeAccepted = 29,
        TradeChanged = 30,
        TradeDone = 31,
        TradeRequested = 32,
        TradeStart = 33,
        Update = 34
    }

    #region Incoming
    public readonly struct NewTick : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.NewTick;
        public readonly int TickId;
        public readonly int TickTime;
        public readonly ObjectStatus[] Statuses;
        public NewTick(Span<byte> buffer, ref int ptr, int len) {
            TickId = PacketUtils.ReadInt(buffer, ref ptr, len);
            TickTime = PacketUtils.ReadInt(buffer, ref ptr, len);
            var length = PacketUtils.ReadUShort(buffer, ref ptr, len);
            Statuses = new ObjectStatus[length];
            for(int i = 0; i < length; i++) {
                Statuses[i] = new ObjectStatus(buffer, ref ptr, len);
            }
        }
        public void Handle() {
            Utils.Log("Handling new tick!");

            PacketHandler.Instance.TickId = TickId;
            PacketHandler.Instance.TickTime = TickTime;

            if(Map.MyPlayer != null) {
                var pos = Map.MyPlayer.GetPosition();
                TcpTicker.Send(new Move(TickId, TickTime, pos.x, pos.y, PacketHandler.Instance.History));
            }

            foreach(var objectStat in Statuses) {
                var entity = Map.Instance.GetEntity(objectStat.Id);
                if (entity == null)
                    continue;
                
                bool isMyPlayer = PacketHandler.Instance.PlayerId == objectStat.Id;
                entity.UpdateObjectStats(objectStat.Stats);
                if (isMyPlayer)
                    continue;

                entity.OnNewTick(objectStat.Position);
            }
        }
    }
    public readonly struct Update : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Update;
        public readonly ObjectDefinition[] Objects;
        public readonly ObjectDrop[] Drops;

        public readonly ushort[] TileTypes;
        public readonly Vector3Int[] TilePositions;
        public Update(Span<byte> buffer, ref int ptr, int len) {
            var tilesLength = PacketUtils.ReadUShort(buffer, ref ptr, len);
            //Utils.Log("Tiles Length: {0}", tilesLength);
            TileTypes = new ushort[tilesLength];
            TilePositions = new Vector3Int[tilesLength];
            for(int i = 0; i <  tilesLength; i++) {
                TilePositions[i] = new Vector3Int(
                    PacketUtils.ReadShort(buffer, ref ptr, len), 
                    PacketUtils.ReadShort(buffer, ref ptr, len)
                );
                TileTypes[i] = PacketUtils.ReadUShort(buffer, ref ptr, len);
            }
            var dropsLength = PacketUtils.ReadUShort(buffer, ref ptr, len);
            Drops = new ObjectDrop[dropsLength];
            //Utils.Log("Drops Length: {0}", dropsLength);
            for(int i = 0; i < dropsLength; i++) {
                Drops[i] = new ObjectDrop(buffer, ref ptr, len);
            }
            var objLength = PacketUtils.ReadUShort(buffer, ref ptr, len);
            Objects = new ObjectDefinition[objLength];

            //Utils.Log("Objects Length: {0}", objLength);
            for(int i =0; i <  objLength; i++) {
                Objects[i] = new ObjectDefinition(buffer, ref ptr, len);
            }

        }
        public void Handle() {
            TcpTicker.Send(new UpdateAck());

            Span<ObjectDrop> drops = Drops.AsSpan();
            for(int i = 0; i < drops.Length; i++) {
                var drop = drops[i];
                Map.Instance.RemoveEntity(drop.Id);
            }

            Map.Instance.SetTiles(TilePositions, TileTypes);

            Span<ObjectDefinition> objects = Objects.AsSpan();
            for(int i = 0; i < objects.Length; i++) {
                var obj = objects[i];

                //Creates entity data and places it at starting position
                var entity = Entity.Resolve(obj);

                //Prepares to add the entity to the world
                Map.Instance.AddEntity(entity);

                if (obj.ObjectStatus.Id == PacketHandler.Instance.PlayerId)
                    Map.Instance.OnMyPlayerConnected(entity as Player);
            }
        }
    }
    public readonly struct AccountList : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.AccountList;
        public AccountList(Span<byte> buffer, ref int ptr, int lenr)
        {

        }
        public void Handle() { }
    }
    public readonly struct AllyShoot : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.AllyShoot;
        public readonly byte BulletId;
        public readonly int OwnerId;
        public readonly ushort ContainerType;
        public readonly float Angle;
        public AllyShoot(Span<byte> buffer, ref int ptr, int len) {
            BulletId = PacketUtils.ReadByte(buffer, ref ptr, len);
            OwnerId = PacketUtils.ReadInt(buffer, ref ptr, len);
            ContainerType = PacketUtils.ReadUShort(buffer, ref ptr, len);
            Angle = PacketUtils.ReadFloat(buffer, ref ptr, len);
        }
        public void Handle() { }
    }
    public readonly struct Aoe : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Aoe;
        public Aoe(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct BuyResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.BuyResult;
        public BuyResult(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct ClientStat : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.ClientStat;
        public ClientStat(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct CreateSuccess : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.CreateSuccess;
        public readonly int ObjectId;
        public readonly int CharacterId;
        public CreateSuccess(Span<byte> buffer, ref int ptr, int len) {
            ObjectId = PacketUtils.ReadInt(buffer, ref ptr, len);
            CharacterId = PacketUtils.ReadInt(buffer, ref ptr, len);
        }
        public void Handle() { }
    }
    public readonly struct Damage : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Damage;
        public Damage(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Death : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Death;
        public Death(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct EnemyShoot : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.EnemyShoot;
        public EnemyShoot(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Failure : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Failure;
        public readonly int ErrorCode;
        public readonly string Description;
        public Failure(Span<byte> buffer, ref int ptr, int len) {
            ErrorCode = PacketUtils.ReadInt(buffer, ref ptr, len);
            Description = PacketUtils.ReadString(buffer, ref ptr, len);
        }
        public void Handle() { }
    }
    public readonly struct File : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.File;
        public File(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct GlobalNotification : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.GlobalNotification;
        public GlobalNotification(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Goto : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Goto;
        public Goto(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct GuildResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.GuildResult;
        public GuildResult(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct InvitedToGuild : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.InvitedToGuild;
        public InvitedToGuild(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct InvResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.InvResult;
        public readonly byte Result;
        public InvResult(Span<byte> buffer, ref int ptr, int len) {
            Result = PacketUtils.ReadByte(buffer, ref ptr, len);
        }
        public void Handle() { }
    }
    public readonly struct MapInfo : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.MapInfo;
        public readonly int Width;
        public readonly int Height;
        public readonly string WorldId;
        public readonly string DisplayName;
        public readonly uint Seed;
        public readonly int Difficulty;
        public readonly int Background;
        public readonly bool AllowTeleport;
        public readonly bool ShowDisplays;
        public readonly int LightColor;
        public readonly float LightIntensity;
        public readonly bool UseIntensity;
        public readonly float DayLightIntensity;
        public readonly float NightLightIntensity;
        public readonly long TotalElapsedMicroSeconds;
        public MapInfo(Span<byte> buffer, ref int ptr, int len)
        {
            Width = PacketUtils.ReadInt(buffer, ref ptr, len);
            Height = PacketUtils.ReadInt(buffer, ref ptr, len);
            WorldId = PacketUtils.ReadString(buffer, ref ptr, len);
            DisplayName = PacketUtils.ReadString(buffer, ref ptr, len);
            Seed = PacketUtils.ReadUInt(buffer, ref ptr, len);
            Difficulty = PacketUtils.ReadInt(buffer, ref ptr, len);
            Background = PacketUtils.ReadInt(buffer, ref ptr, len);
            AllowTeleport = PacketUtils.ReadBool(buffer, ref ptr, len);
            ShowDisplays = PacketUtils.ReadBool(buffer, ref ptr, len);
            LightColor = PacketUtils.ReadInt(buffer, ref ptr, len);
            LightIntensity = PacketUtils.ReadFloat(buffer, ref ptr, len);
            UseIntensity = PacketUtils.ReadBool(buffer, ref ptr, len);

            Utils.Log("MapInfo::UseIntensity::{0}", UseIntensity);
            //UseIntensity is true even though it shouldnt be, heck?!
            //TODO Still need to add lights
            //if (UseIntensity) {
            //    DayLightIntensity = PacketUtils.ReadFloat(buffer, ref ptr, len);
            //    NightLightIntensity = PacketUtils.ReadFloat(buffer, ref ptr, len);
            //    TotalElapsedMicroSeconds = PacketUtils.ReadLong(buffer, ref ptr, len);
            //}
            //else {
                DayLightIntensity = 0;
                NightLightIntensity = 0;
                TotalElapsedMicroSeconds = 0;
            //}
        }
        public void Handle() {
            Map.Instance.Init(this);
            PacketHandler.Instance.Random = new wRandom(Seed);
        }
    }
    public readonly struct NameResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.NameResult;
        public NameResult(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Notification : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Notification;
        public Notification(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Pic : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Pic;
        public Pic(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Ping : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Ping;
        public readonly int Serial;
        public Ping(Span<byte> buffer, ref int ptr, int len) {
            Serial = PacketUtils.ReadInt(buffer, ref ptr, len);
        }
        public void Handle() {
            Utils.Log("Handling Ping");
            TcpTicker.Send(new Pong(Serial, GameTime.ElapsedMicroseconds));
        }
    }
    public readonly struct PlaySound : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.PlaySound;
        public PlaySound(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct QuestObjId : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.QuestObjId;
        public QuestObjId(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Reconnect : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Reconnect;
        public Reconnect(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct ServerPlayerShoot : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.ServerPlayerShoot;
        public ServerPlayerShoot(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct ShowEffect : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.ShowEffect;
        public ShowEffect(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeStart : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeStart;
        public TradeStart(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeRequested : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeRequested;
        public TradeRequested(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeDone : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeDone;
        public TradeDone(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeChanged : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeChanged;
        public TradeChanged(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeAccepted : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeAccepted;
        public TradeAccepted(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { }
    }
    public readonly struct Text : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Text;
        public Text(Span<byte> buffer, ref int ptr, int len)
        {

        }
        public void Handle() { 
        
        }
    }
    #endregion
    #region Outgoing
    public readonly struct JoinGuild : IOutgoingPacket {
        public C2SPacketId Id => C2SPacketId.JoinGuild;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct ChooseName : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ChooseName;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct UsePortal : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.UsePortal;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct UseItem : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.UseItem;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct UpdateAck : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.UpdateAck;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct UnknownOut: IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Unknown;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct Teleport : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Teleport;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct SquareHit: IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.SquareHit;
        public void Write(Span<byte> buffer, ref int ptr)
        {

        }
    }
    public readonly struct ShootAck : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ShootAck;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct Reskin : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Reskin;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct RequestTrade: IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.RequestTrade;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct Pong : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Pong;
        public readonly int Serial;
        public readonly long Time;
        public Pong(int serial, long time) {
            Serial = serial;
            Time = time;
        }
        public void Write(Span<byte> buffer, ref int ptr) {
            PacketUtils.WriteInt(buffer, Serial, ref ptr);
            PacketUtils.WriteLong(buffer, Time, ref ptr);
        }
    }
    public readonly struct PlayerText : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.PlayerText;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct PlayerShoot : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.PlayerShoot;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct PlayerHit : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.PlayerHit;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct OtherHit : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.OtherHit;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct Move : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Move;
        public readonly int TickId;
        public readonly long TickTime;
        public readonly float X;
        public readonly float Y;
        public readonly MoveRecord[] History;
        public Move(int tickId, long tickTime, float x, float y, MoveRecord[] history)
        {
            TickId = tickId;
            TickTime = tickTime;
            X = x;
            Y = y;
            History = history;
        }
        public void Write(Span<byte> buffer, ref int ptr)
        {
            //wtr.Write(TickId);
            //wtr.Write(TickTime);
            //wtr.Write(X);
            //wtr.Write(Y);
            //wtr.Write((ushort)History.Length);
            //for (int i = 0; i < History.Length; i++)
            //    History[i].Write(wtr);

        }
    }
    public readonly struct InvSwap : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.InvSwap;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct InvDrop : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.InvDrop;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct Hello : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Hello;
        public readonly ushort MajorVersion;
        public readonly ushort MinorVersion;
        public readonly int WorldId;
        public readonly string Email;
        public readonly string Password;
        public readonly short CharId;
        public readonly bool CreateChar;
        public readonly short CharType;
        public readonly short SkinType;
        public Hello(ushort majorVersion, ushort minorVersion, int worldId, string email, string password, short charId, bool createChar, short charType, short skinType)
        {
            MajorVersion = majorVersion;
            MinorVersion = minorVersion;
            WorldId = worldId;
            Email = email;
            Password = password;
            CharId = charId;
            CreateChar = createChar;
            CharType = charType;
            SkinType = skinType;
        }
        public void Write(Span<byte> buffer, ref int ptr) {
            PacketUtils.WriteUShort(buffer, MajorVersion, ref ptr);
            PacketUtils.WriteUShort(buffer, MinorVersion, ref ptr);
            PacketUtils.WriteInt(buffer, WorldId, ref ptr);
            PacketUtils.WriteString(buffer, Email, ref ptr);
            PacketUtils.WriteString(buffer, Password, ref ptr);
            PacketUtils.WriteShort(buffer, CharId, ref ptr);
            PacketUtils.WriteBool(buffer, CreateChar, ref ptr);
            PacketUtils.WriteShort(buffer, CharType, ref ptr);
            PacketUtils.WriteShort(buffer, SkinType, ref ptr);
        }
    }
    public readonly struct GuildRemove : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.GuildRemove;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct GuildInvite : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.GuildInvite;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct GroundDamage : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.GroundDamage;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct Escape : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Escape;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct EnemyHit : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.EnemyHit;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct EditAccountList : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.EditAccountList;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct CreateGuild : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.CreateGuild;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct ChangeTrade : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ChangeTrade;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct ChangeGuildRank : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ChangeGuildRank;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct CancelTrade : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.CancelTrade;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct Buy : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Buy;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct AoeAck : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.AoeAck;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    public readonly struct AcceptTrade : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.AcceptTrade;
        public void Write(Span<byte> buffer, ref int ptr) { }
    }
    #endregion
}