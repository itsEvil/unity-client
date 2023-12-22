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
using Ping = Networking.Tcp.Ping;

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
                Settings.GameBuild, Settings.NexusId, 
                AccountData.GetEmail(), AccountData.GetPassword(), 
                InitData.CharId, InitData.NewCharacter, 
                InitData.ClassType, InitData.SkinType));
    }

    public void Stop() {
        TcpTicker.Stop(nameof(PacketHandler));
    }

    public void Tick() {
        while (_toBeHandled.TryDequeue(out var packet)) {
            //Utils.Log("Handing {0} packet", packet.Id);

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

    public void ReadPacket(S2CPacketId id, Span<byte> data, ref int ptr, int len) {
        var packet = CreatePacket(id, data, ref ptr, len);

        if (packet == null)
            return;

        //Utils.Log("Read {0} packet", packet.Id);
        
        AddPacket(packet);
    }

    private IIncomingPacket CreatePacket(S2CPacketId id, Span<byte> data, ref int ptr, int len) {
        IIncomingPacket packet = null;
        switch (id) {
            case S2CPacketId.Ping:
                packet = new Ping(data, ref ptr, len);
                break;
            case S2CPacketId.Failure:
                packet = new Failure(data, ref ptr, len);
                break;
            case S2CPacketId.CreateSuccess:
                packet = new CreateSuccess(data, ref ptr, len);
                break;
            case S2CPacketId.Text:
                packet = new Text(data, ref ptr, len);
                break;
            case S2CPacketId.ServerPlayerShoot:
                packet = new ServerPlayerShoot(data, ref ptr, len);
                break;
            case S2CPacketId.Damage:
                packet = new Damage(data, ref ptr, len);
                break;
            case S2CPacketId.Notification:
                packet = new Notification(data, ref ptr, len);
                break;
            case S2CPacketId.NewTick:
                packet = new NewTick(data, ref ptr, len);
                break;
            case S2CPacketId.ShowEffect:
                packet = new ShowEffect(data, ref ptr, len);
                break;
            case S2CPacketId.Goto:
                packet = new Goto(data, ref ptr, len);
                break;
            case S2CPacketId.Reconnect:
                packet = new Reconnect(data, ref ptr, len);
                break;
            case S2CPacketId.MapInfo:
                packet = new MapInfo(data, ref ptr, len);
                break;
            case S2CPacketId.Death:
                packet = new Death(data, ref ptr, len);
                break;
            case S2CPacketId.Aoe:
                packet = new Aoe(data, ref ptr, len);
                break;
            case S2CPacketId.AccountList:
                packet = new AccountList(data, ref ptr, len);
                break;
            case S2CPacketId.QuestObjId:
                packet = new QuestObjId(data, ref ptr, len);
                break;
            case S2CPacketId.AllyShoot:
                packet = new AllyShoot(data, ref ptr, len);
                break;
            case S2CPacketId.EnemyShoot:
                packet = new EnemyShoot(data, ref ptr, len);
                break;
            case S2CPacketId.PlaySound:
                packet = new PlaySound(data, ref ptr, len);
                break;
            case S2CPacketId.GlobalNotification:
                packet = new GlobalNotification(data, ref ptr, len);
                break;
            case S2CPacketId.Update:
                packet = new Update(data, ref ptr, len);
                break;
        }

        return packet;
    }
}

