using Networking.Tcp;
using Networking;
using Static;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public sealed class Interactive : Entity
{
    public override void Init(ObjectDesc desc, ObjectDefinition defi) {
        base.Init(desc, defi);
        Type = GameObjectType.Interactive;
        IsInteractive = true;
    }
    public override bool Tick() {
        return base.Tick();
    }
    public override void Dispose() {
        base.Dispose();
    }
    public void Interact() {
        switch (Descriptor.Class) {
            case ObjectType.CharacterChanger:
                break;
            case ObjectType.ConnectedWall:
                break;
            case ObjectType.Container:
                break;
            case ObjectType.GuildChronicle:
                break;
            case ObjectType.GuildHallPortal:
                break;
            case ObjectType.GuildRegister:
                break;
            case ObjectType.GuildMerchant:
                break;
            case ObjectType.GuildBoard:
                break;
            case ObjectType.MarketObject:
                break;
            case ObjectType.Merchant:
                break;
            case ObjectType.NameChanger:
                break;
            case ObjectType.OneWayContainer:
                break;
            case ObjectType.Portal:
                TcpTicker.Send(new UsePortal(Id));
                break;
            case ObjectType.ReskinVendor:
                break;
            case ObjectType.VaultChest:
                break;
            case ObjectType.ClosedVaultChest:
                break;
            case ObjectType.ClosedGiftChest:
                break;
            case ObjectType.MoneyChanger:
                break;
            case ObjectType.PetUpgrader:
                break;
        }
    }
    public void SetWidgetVisibility(bool value) {
        Utils.Log("Setting visibility to {0} for object class {1}", value, Descriptor.Class);
        switch (Descriptor.Class) {
            case ObjectType.CharacterChanger:
                break;
            case ObjectType.ConnectedWall:
                break;
            case ObjectType.Container:
                break;
            case ObjectType.GuildChronicle:
                break;
            case ObjectType.GuildHallPortal:
                break;
            case ObjectType.GuildRegister:
                break;
            case ObjectType.GuildMerchant:
                break;
            case ObjectType.GuildBoard:
                break;
            case ObjectType.MarketObject:
                break;
            case ObjectType.Merchant:
                break;
            case ObjectType.NameChanger:
                break;
            case ObjectType.OneWayContainer:
                break;
            case ObjectType.Portal:
                GameScreenController.Instance.SetPortalVisiblity(value);
                if (value)
                    GameScreenController.Instance.InitPortalWidget(this);
                break;
            case ObjectType.ReskinVendor:
                break;
            case ObjectType.VaultChest:
                break;
            case ObjectType.ClosedVaultChest:
                break;
            case ObjectType.ClosedGiftChest:
                break;
            case ObjectType.MoneyChanger:
                break;
            case ObjectType.PetUpgrader:
                break;
        }
    }
}
