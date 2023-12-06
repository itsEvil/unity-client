using Account;
using Static;
using System;

public sealed class Player : Entity {

    public int CurrentExp;
    public int NextLevelExp;
    public int Level;
    public int Fame;
    public int Credits;
    public int NumStars;
    public string GuildName = string.Empty;
    public int GuildRank;
    public bool HasBackpack;
    public short SkinType;
    public ItemType[] SlotTypes;
    public int Mp;
    public int MaxMp;

    /// <summary>
    /// Runs when any player gets initalized
    /// </summary>
    public override void Init(ObjectDesc desc) {
        base.Init(desc);
        SlotTypes = new ItemType[Inventory.Length];
        Type = GameObjectType.Player;
    }
    /// <summary>
    /// Only called when our player connects
    /// </summary>
    public void OnMyPlayer() {
        var charStats = AccountData.Characters[AccountData.CurrentCharId];
        Inventory = charStats.Inventory;
    }
    public override void UpdateStat(StatType stat, object value) {
        base.UpdateStat(stat, value);
        switch (stat) {
            case StatType.Exp:
                CurrentExp = (int)value;
                return;
            case StatType.NextLevelExp:
                NextLevelExp = (int)value;
                return;
            case StatType.Level:
                Level = (int)value;
                return;
            case StatType.Fame:
                //account fame ?
                Fame = (int) value;
                return;
            case StatType.NumStars:
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
            case StatType.Mp:
                Mp = (int)value;
                return;
            case StatType.MaxMp:
                MaxMp = (int)value;
                return;
        }
    }
    public override bool Tick() {
        return base.Tick();
    }
    public void OnMove() {

    }
    public override void Dispose() {
        base.Dispose();
    }
}
