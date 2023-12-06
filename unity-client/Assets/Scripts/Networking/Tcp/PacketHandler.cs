using Account;
using Game;
using Networking;
using Networking.Tcp;
using Networking.Web;
using Static;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PacketHandler {
    public static PacketHandler Instance { get; private set; }
    private ConcurrentQueue<IIncomingPacket> _toBeHandled;

    public int PlayerId;
    public int TickId;
    public long TickTime;
    public MoveRecord[] History = new MoveRecord[10];
    public readonly GameInitData InitData;
    public wRandom Random;

    public PacketHandler(GameInitData initData) {
        Instance = this;
        InitData = initData;
    }
    public void Start() {
        for(int i = 0; i < History.Length; i++) {
            History[i] = new MoveRecord();
        }

        Instance = this;
        _toBeHandled = new ConcurrentQueue<IIncomingPacket>();
        TcpTicker.Start(this);
        TcpTicker.Send(
            new Hello(
                Settings.GameVersion, Settings.NexusId, 
                AccountData.GetEmail(), AccountData.GetPassword(), 
                InitData.CharId, InitData.NewCharacter, 
                InitData.ClassType, InitData.SkinType));
    }

    public void Stop() {
        TcpTicker.Stop();
    }

    public void Tick() {
        while (_toBeHandled.TryDequeue(out var packet)) {
            Utils.Log("Handing {0} packet", packet.Id);

            packet.Handle();

            if (!TcpTicker.Running) {
                Utils.Error("Lost connection");
                return;
            }
        }
    }

    public void AddPacket(IIncomingPacket packet) {
        _toBeHandled.Enqueue(packet);
    }

    public void ReadPacket(S2CPacketId id, byte[] data) {
        var packet = CreatePacket(id, data);

        if (packet == null)
            return;

        Utils.Log("Read {0} packet", packet.Id);
        
        AddPacket(packet);
    }

    private IIncomingPacket CreatePacket(S2CPacketId id, byte[] data) {
        IIncomingPacket packet = null;
        using var rdr = new PacketReader(new MemoryStream(data));
        switch (id) {
            case S2CPacketId.Failure:
                packet = new Failure(rdr);
                break;
            case S2CPacketId.CreateSuccess:
                packet = new CreateSuccess(rdr);
                break;
            case S2CPacketId.Text:
                packet = new Text(rdr);
                break;
            case S2CPacketId.ServerPlayerShoot:
                packet = new ServerPlayerShoot(rdr);
                break;
            case S2CPacketId.Damage:
                packet = new Damage(rdr);
                break;
            case S2CPacketId.Notification:
                packet = new Notification(rdr);
                break;
            case S2CPacketId.NewTick:
                packet = new NewTick(rdr);
                break;
            case S2CPacketId.ShowEffect:
                packet = new ShowEffect(rdr);
                break;
            case S2CPacketId.Goto:
                packet = new Goto(rdr);
                break;
            case S2CPacketId.Reconnect:
                packet = new Reconnect(rdr);
                break;
            case S2CPacketId.MapInfo:
                packet = new MapInfo(rdr);
                break;
            case S2CPacketId.Death:
                packet = new Death(rdr);
                break;
            case S2CPacketId.Aoe:
                packet = new Aoe(rdr);
                break;
            case S2CPacketId.AccountList:
                packet = new AccountList(rdr);
                break;
            case S2CPacketId.QuestObjId:
                packet = new QuestObjId(rdr);
                break;
            case S2CPacketId.AllyShoot:
                packet = new AllyShoot(rdr);
                break;
            case S2CPacketId.EnemyShoot:
                packet = new EnemyShoot(rdr);
                break;
            case S2CPacketId.PlaySound:
                packet = new PlaySound(rdr);
                break;
            case S2CPacketId.GlobalNotification:
                packet = new GlobalNotification(rdr);
                break;
            case S2CPacketId.Update:
                packet = new Update(rdr);
                break;
        }

        return packet;
    }
}

