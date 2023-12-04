using Networking.Tcp;
using Networking.Web;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading.Tasks;

namespace Networking {
    public enum SocketEventState {
        Awaiting,
        InProgress
    }

    public static class TcpTicker {
        private const int BUFFER_SIZE = 0x50000;
        private const int PREFIX_LENGTH = 5;
        private const int PREFIX_LENGTH_WITH_ID = PREFIX_LENGTH - 1;

        private static Task _tickingTask;
        private static bool _crashed;
        public static bool Running => _tickingTask != null && !_tickingTask.IsCompleted;

        private static readonly SendState _Send = new();
        private static readonly ReceiveState _Receive = new();

        private static Socket _socket;

        private static ConcurrentQueue<IOutgoingPacket> _pending;

        private static PacketHandler _packetHandler;
        public static void Start(PacketHandler packetHandler) {
            if (Running) {
                Utils.Warn("TcpTicker already started");
                return;
            }

            //ServerInfo serverInfo = (ServerInfo)GameConfiguration.ServerInfos[GameConfiguration.SelectedServer];
            try {
                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(Settings.GAME_ADDRESS, Settings.GAME_PORT);
                _socket.NoDelay = true;
                _socket.Blocking = false;
            }
            catch (Exception) {
                //ViewManager.Instance.ChangeView(View.Menu);
                return;
            }

            Utils.Log("Tcp Connected!");

            _packetHandler = packetHandler;
            _pending = new ConcurrentQueue<IOutgoingPacket>();
            _crashed = false;
            _tickingTask = Task.Run(Tick);
        }

        // called on main thread
        public static void Stop() {
            if (!Running && !_crashed)
                return;

            _Send.Reset();
            _Receive.Reset();
            _tickingTask = null;
            _crashed = false;

            try {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception e) {
                Utils.Log(e.Message);
            }

            Utils.Log("Tcp Disconnected!");
        }

        // called on main thread
        public static void Send(IOutgoingPacket packet) {
            if (!Running) {
                //SpriteUtils.ClearCache(SpriteUtils.CacheType.All);
                //ViewManager.Instance.ChangeView(View.Menu);
                return;
            }

            _pending.Enqueue(packet);
        }

        // called on worker thread
        private static void Tick() {
            try {
                while (Running) {
                    StartSend();
                    StartReceive();
                }
            }
            catch (Exception e) {
                Utils.Error("TcpTicker::", e.Message ,e.StackTrace);
                _crashed = true;
            }
        }

        // called on worker thread
        private static void StartSend() {
            switch (_Send.State) {
                case SocketEventState.Awaiting:
                    if (_pending.TryDequeue(out var packet)) {
                        using (var wtr = new PacketWriter(new MemoryStream())) {
                            //Debug.Log($"TcpTicker::SentPacket::{packet.Id}");
                            wtr.Write((byte)packet.Id);
                            packet.Write(wtr);

                            var bytes = ((MemoryStream)wtr.BaseStream).ToArray();
                            _Send.PacketBytes = bytes;
                            _Send.PacketLength = bytes.Length;
                        }
                        _Send.State = SocketEventState.InProgress;
                        StartSend();
                    }
                    break;
                case SocketEventState.InProgress:
                    Buffer.BlockCopy(_Send.PacketBytes, 0, _Send.Data, PREFIX_LENGTH_WITH_ID, _Send.PacketLength);
                    Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(_Send.PacketLength + PREFIX_LENGTH_WITH_ID)), 0, _Send.Data, 0, PREFIX_LENGTH_WITH_ID);
                    var written = _socket.Send(_Send.Data, _Send.BytesWritten, _Send.PacketLength + PREFIX_LENGTH_WITH_ID - _Send.BytesWritten, SocketFlags.None);
                    if (written < _Send.PacketLength + PREFIX_LENGTH_WITH_ID)
                        _Send.BytesWritten += written;
                    else
                        _Send.Reset();
                    StartSend();
                    break;
            }
        }

        // called on worker thread
        private static void StartReceive() {
            switch (_Receive.State) {
                case SocketEventState.Awaiting:
                    if (_socket.Available >= PREFIX_LENGTH) {
                        _socket.Receive(_Receive.PacketBytes, PREFIX_LENGTH, SocketFlags.None);
                        _Receive.PacketLength = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(_Receive.PacketBytes, 0));
                        _Receive.State = SocketEventState.InProgress;
                        StartReceive();
                    }
                    break;
                case SocketEventState.InProgress:
                    if (_Receive.PacketLength < PREFIX_LENGTH ||
                        _Receive.PacketLength > BUFFER_SIZE) {
                        throw new Exception($"Unable to process packet of size {_Receive.PacketLength}");
                    }
                    //Full packet now arrived. Time to process it.
                    if (_socket.Available + PREFIX_LENGTH >= _Receive.PacketLength) {
                        if (_socket.Available != 0)
                            _socket.Receive(_Receive.PacketBytes, PREFIX_LENGTH, _Receive.PacketLength - PREFIX_LENGTH, SocketFlags.None);
                        var packetId = (S2CPacketId)_Receive.GetPacketId();
                        var packetBody = _Receive.GetPacketBody();

                        _packetHandler.ReadPacket(packetId, packetBody);
                        _Receive.Reset();
                    }

                    StartReceive();
                    break;
            }
        }

        private class ReceiveState {
            public int PacketLength;
            public readonly byte[] PacketBytes;
            public SocketEventState State;

            public ReceiveState() {
                PacketBytes = new byte[BUFFER_SIZE];
                PacketLength = PREFIX_LENGTH;
            }
            public byte[] GetPacketBody() {
                var packetBody = new byte[PacketLength - PREFIX_LENGTH];
                Array.Copy(PacketBytes, PREFIX_LENGTH, packetBody, 0, packetBody.Length);
                return packetBody;
            }
            public int GetPacketId() {
                return PacketBytes[4];
            }
            public void Reset() {
                State = SocketEventState.Awaiting;
                PacketLength = 0;
            }
        }

        private class SendState {
            public int BytesWritten;
            public int PacketLength;
            public byte[] PacketBytes;
            public SocketEventState State;
            public readonly byte[] Data;
            public SendState() {
                Data = new byte[0x10000];
            }
            public void Reset() {
                State = SocketEventState.Awaiting;
                PacketLength = 0;
                BytesWritten = 0;
                PacketBytes = null;
            }
        }
    }
}