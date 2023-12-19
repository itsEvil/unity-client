using Networking.Tcp;
using Networking.Web;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System;
using System.Threading.Tasks;
using System.Buffers.Binary;
using System.Text;
using UnityEditor.Experimental.Rendering;
using System.Threading;

namespace Networking {
    public enum SocketEventState {
        Awaiting,
        InProgress
    }

    public static class TcpTicker {

        public const int LENGTH_PREFIX = 2;

        private const int BUFFER_SIZE = ushort.MaxValue;
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
        private static CancellationTokenSource _tokenSource = new();
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
            catch (Exception e) {
                Utils.Error("FailedToConnectViaTcp {0}\n{1}", e.Message, e.StackTrace);
                //ViewManager.Instance.ChangeView(View.Menu);
                return;
            }

            Utils.Log("Tcp Connected!");

            _tokenSource.Dispose();
            _tokenSource = new();

            _packetHandler = packetHandler;
            _pending = new ConcurrentQueue<IOutgoingPacket>();
            _crashed = false;
            _tickingTask = Task.Run(Tick);
        }

        // called on main thread
        public static void Stop(string msg) {
            Utils.Log("Trying to disconnect tcp from {0}", msg);

            _tokenSource.Cancel();

            //if (!Running && !_crashed)
            //    return;

            try {
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
            catch (Exception e) {
                Utils.Log(e.Message);
            }

            _Send.Reset();
            _Receive.Reset();
            _tickingTask = null;
            _crashed = false;

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
                while (Running && !_tokenSource.IsCancellationRequested) {
                    if (!_socket.Connected)
                        return;

                    Utils.Log("TcpTicker::Tick");

                    StartSend();
                    StartReceive();
                }
            }
            catch (Exception e) {
                Utils.Error("TcpTicker::{0}::{1}", e.Message , e.StackTrace);
                Stop(nameof(Tick));
                //_crashed = true;
            }
        }

        // called on worker thread
        private static async void StartSend() {
            try
            {
                while(_pending.TryDequeue(out var packet) && !_tokenSource.IsCancellationRequested) {
                    int ptr = LENGTH_PREFIX;
                    PacketUtils.WriteByte(_Send.Data.AsSpan()[ptr..], (byte)packet.Id, ref ptr);
                    packet.Write(_Send.Data.AsSpan()[(ptr - 3)..], ref ptr);
                    _Send.PacketLength = ptr;

                        Utils.Log($"Sending packet {(C2SPacketId)_Send.Data[LENGTH_PREFIX]} {_Send.Data[LENGTH_PREFIX]} {_Send.PacketLength}");                    
                        BinaryPrimitives.WriteUInt16LittleEndian(_Send.Data.AsSpan()[0..], (ushort)_Send.PacketLength); //Length
                        
                        //Debug data
                        //StringBuilder sb = new();
                        //for(int i = 0; i < _Send.PacketLength; i++)
                        //    sb.Append('[').Append(_Send.Data[i]).Append(']');
                        //                
                        //Utils.Log("Sending | Length: {0} Bytes: {1}", _Send.PacketLength, sb.ToString());
                        //End of Debug

                        _ = await _socket.SendAsync(new ArraySegment<byte>(_Send.Data, 0, _Send.PacketLength), SocketFlags.None);
                    }
                }

            catch (Exception e) {
                throw e;
                //Stop(nameof(StartSend));
                //Utils.Error("Send Error {0}", e);
            }
        }

        // called on worker thread
        private static async void StartReceive() {
            var len = await _socket.ReceiveAsync(new ArraySegment<byte>(_Receive.PacketBytes), SocketFlags.None);

            if (len == 0)
                throw new Exception("Data received length is zero");
            
            if (len < 0)
                return;

            ProcessPacket(len);
        }
        private static void ProcessPacket(int len) {
            try
            {
                int ptr = 0;
                var buffer = _Receive.PacketBytes.AsSpan();

                //while(ptr < len && !_tokenSource.IsCancellationRequested) {
                var packetLen = PacketUtils.ReadUShort(buffer, ref ptr, len);

                if(len != packetLen) {
                    Utils.Warn("Packet length miss match {0} != {1}", len, packetLen);
                }

                var nextPacketPtr = ptr + packetLen - 2;
                var packetId = (S2CPacketId)PacketUtils.ReadByte(buffer, ref ptr, nextPacketPtr);
                Utils.Log("Received packet {0}", packetId);

                PacketHandler.Instance.ReadPacket(packetId, buffer, ref ptr, nextPacketPtr);

                ptr = nextPacketPtr;

                //}
            }
            catch(Exception e) {
                Utils.Error("Read exception {0}::{1}", e.Message, e.StackTrace);
                Stop(nameof(ProcessPacket));
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
                Data = new byte[ushort.MaxValue];
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