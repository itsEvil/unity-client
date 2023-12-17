using Game;
using Static;
using System;

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
        public NewTick(PacketReader rdr)
        {
            TickId = rdr.ReadInt32();
            TickTime = rdr.ReadInt32();
            var length = rdr.ReadUInt16();
            Statuses = new ObjectStatus[length];
            for(int i = 0; i<length; i++) {
                Statuses[i] = new ObjectStatus(rdr);
            }
        }
        public void Handle() {
            PacketHandler.Instance.TickId = TickId;
            PacketHandler.Instance.TickTime = TickTime;
        }
    }
    public readonly struct Update : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Update;
        public readonly TileData[] Tiles;
        public readonly ObjectDefinition[] Objects;
        public readonly ObjectDrop[] Drops;
        public Update(PacketReader rdr)
        {
            var tilesLength = rdr.ReadUInt16();
            Tiles = new TileData[tilesLength];
            for(int i = 0; i <  tilesLength; i++) {
                Tiles[i] = new TileData(rdr);
            }
            var dropsLength = rdr.ReadUInt16();
            Drops = new ObjectDrop[dropsLength];
            for(int i = 0; i < dropsLength; i++) {
                Drops[i] = new ObjectDrop(rdr);
            }
            var objLength = rdr.ReadUInt16();
            Objects = new ObjectDefinition[objLength];
            for(int i =0; i <  objLength; i++) {
                Objects[i] = new ObjectDefinition(rdr);
            }
        }
        public void Handle() {

            Span<ObjectDefinition> objects = Objects.AsSpan();
            for(int i = 0; i < objects.Length; i++) {
                var obj = objects[i];

                //Creates entity data and places it at starting position
                var entity = Entity.Resolve(obj);

                //Prepares to add the entity to the world
                Map.Instance.AddEntity(entity);
            }
        }
    }
    public readonly struct AccountList : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.AccountList;
        public AccountList(PacketReader rdr)
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
        public AllyShoot(PacketReader rdr) {
            BulletId = rdr.ReadByte();
            OwnerId = rdr.ReadInt32();
            ContainerType = rdr.ReadUInt16();
            Angle = rdr.ReadSingle();
        }
        public void Handle() { }
    }
    public readonly struct Aoe : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Aoe;
        public Aoe(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct BuyResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.BuyResult;
        public BuyResult(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct ClientStat : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.ClientStat;
        public ClientStat(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct CreateSuccess : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.CreateSuccess;
        public readonly int ObjectId;
        public readonly int CharacterId;
        public CreateSuccess(PacketReader rdr) {
            ObjectId = rdr.ReadInt32();
            CharacterId = rdr.ReadInt32();
        }
        public void Handle() { }
    }
    public readonly struct Damage : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Damage;
        public Damage(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Death : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Death;
        public Death(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct EnemyShoot : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.EnemyShoot;
        public EnemyShoot(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Failure : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Failure;
        public readonly int ErrorCode;
        public readonly string Description;
        public Failure(PacketReader rdr) {
            ErrorCode = rdr.ReadInt32();
            Description = rdr.ReadString();
        }
        public void Handle() { }
    }
    public readonly struct File : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.File;
        public File(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct GlobalNotification : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.GlobalNotification;
        public GlobalNotification(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Goto : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Goto;
        public Goto(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct GuildResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.GuildResult;
        public GuildResult(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct InvitedToGuild : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.InvitedToGuild;
        public InvitedToGuild(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct InvResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.InvResult;
        public readonly byte Result;
        public InvResult(PacketReader rdr) {
            Result = rdr.ReadByte();
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
        public MapInfo(PacketReader rdr)
        {
            Width = rdr.ReadInt32();
            Height = rdr.ReadInt32();
            WorldId = rdr.ReadString();
            DisplayName = rdr.ReadString();
            Seed = rdr.ReadUInt32();
            Difficulty = rdr.ReadInt32();
            Background = rdr.ReadInt32();
            AllowTeleport = rdr.ReadBoolean();
            ShowDisplays = rdr.ReadBoolean();
            LightColor = rdr.ReadInt32();
            LightIntensity = rdr.ReadSingle();
            UseIntensity = rdr.ReadBoolean();
            DayLightIntensity = rdr.ReadSingle();
            NightLightIntensity = rdr.ReadSingle();
            TotalElapsedMicroSeconds = rdr.ReadInt64();
        }
        public void Handle() {
            Map.Instance.Init(this);
            PacketHandler.Instance.Random = new wRandom(Seed);
        }
    }
    public readonly struct NameResult : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.NameResult;
        public NameResult(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Notification : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Notification;
        public Notification(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Pic : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Pic;
        public Pic(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Ping : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Ping;
        public readonly int Serial;
        public Ping(PacketReader rdr) {
            Serial = rdr.ReadInt32();
        }
        public void Handle() { }
    }
    public readonly struct PlaySound : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.PlaySound;
        public PlaySound(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct QuestObjId : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.QuestObjId;
        public QuestObjId(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Reconnect : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Reconnect;
        public Reconnect(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct ServerPlayerShoot : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.ServerPlayerShoot;
        public ServerPlayerShoot(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct ShowEffect : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.ShowEffect;
        public ShowEffect(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeStart : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeStart;
        public TradeStart(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeRequested : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeRequested;
        public TradeRequested(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeDone : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeDone;
        public TradeDone(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeChanged : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeChanged;
        public TradeChanged(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct TradeAccepted : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.TradeAccepted;
        public TradeAccepted(PacketReader rdr)
        {

        }
        public void Handle() { }
    }
    public readonly struct Text : IIncomingPacket {
        public S2CPacketId Id => S2CPacketId.Text;
        public Text(PacketReader rdr)
        {

        }
        public void Handle() { 
        
        }
    }
    #endregion
    #region Outgoing
    public readonly struct JoinGuild : IOutgoingPacket {
        public C2SPacketId Id => C2SPacketId.JoinGuild;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct ChooseName : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ChooseName;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct UsePortal : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.UsePortal;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct UseItem : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.UseItem;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct UpdateAck : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.UpdateAck;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct UnknownOut: IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Unknown;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct Teleport : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Teleport;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct SquareHit: IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.SquareHit;
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
        }
    }
    public readonly struct ShootAck : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ShootAck;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct Reskin : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Reskin;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct RequestTrade: IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.RequestTrade;
        public void Write(PacketWriter wtr) { }
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
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
            wtr.Write(Serial);
            wtr.Write(Time);
        }
    }
    public readonly struct PlayerText : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.PlayerText;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct PlayerShoot : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.PlayerShoot;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct PlayerHit : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.PlayerHit;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct OtherHit : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.OtherHit;
        public void Write(PacketWriter wtr) { }
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
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
            wtr.Write(TickId);
            wtr.Write(TickTime);
            wtr.Write(X);
            wtr.Write(Y);
            wtr.Write((ushort)History.Length);
            for (int i = 0; i < History.Length; i++)
                History[i].Write(wtr);
        }
    }
    public readonly struct InvSwap : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.InvSwap;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct InvDrop : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.InvDrop;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct Hello : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Hello;
        public readonly string GameVersion;
        public readonly int WorldId;
        public readonly string Email;
        public readonly string Password;
        public readonly short CharId;
        public readonly bool CreateChar;
        public readonly short CharType;
        public readonly short SkinType;
        public Hello(string version, int worldId, string email, string password, short charId, bool createChar, short charType, short skinType)
        {
            GameVersion = version;
            WorldId = worldId;
            Email = email;
            Password = password;
            CharId = charId;
            CreateChar = createChar;
            CharType = charType;
            SkinType = skinType;
        }
        public void Write(PacketWriter wtr) {
            wtr.Write((byte)Id);
            wtr.Write(GameVersion);
            wtr.Write(WorldId);
            wtr.Write(Email);
            wtr.Write(Password);
            wtr.Write(CharId);
            wtr.Write(CreateChar);
            wtr.Write(CharType);
            wtr.Write(SkinType);
        }
    }
    public readonly struct GuildRemove : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.GuildRemove;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct GuildInvite : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.GuildInvite;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct GroundDamage : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.GroundDamage;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct Escape : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Escape;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct EnemyHit : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.EnemyHit;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct EditAccountList : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.EditAccountList;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct CreateGuild : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.CreateGuild;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct ChangeTrade : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ChangeTrade;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct ChangeGuildRank : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.ChangeGuildRank;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct CancelTrade : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.CancelTrade;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct Buy : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.Buy;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct AoeAck : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.AoeAck;
        public void Write(PacketWriter wtr) { }
    }
    public readonly struct AcceptTrade : IOutgoingPacket
    {
        public C2SPacketId Id => C2SPacketId.AcceptTrade;
        public void Write(PacketWriter wtr) { }
    }
    #endregion
}