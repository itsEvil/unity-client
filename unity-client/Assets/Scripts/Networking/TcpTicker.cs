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
using UnityEditor.PackageManager.UI;

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
        private static CancellationTokenSource _tokenSource;
        private static SocketAsyncEventArgs _socketArgs;
        public static void Start(PacketHandler packetHandler) {
            if (Running) {
                Utils.Warn("TcpTicker already started");
                return;
            }

            _socketArgs = new();
            _socketArgs.SetBuffer(_Receive.PacketBytes);
            //ServerInfo serverInfo = (ServerInfo)GameConfiguration.ServerInfos[GameConfiguration.SelectedServer];
            try {
                _socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(Settings.GAME_ADDRESS, Settings.GAME_PORT);
                _socket.NoDelay = true;
                _socket.Blocking = true;
            }
            catch (Exception e) {
                Utils.Error("FailedToConnectViaTcp {0}\n{1}", e.Message, e.StackTrace);
                //ViewManager.Instance.ChangeView(View.Menu);
                return;
            }

            Utils.Log("Tcp Connected!");
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
            _tickingTask.Wait(50);

            //if (!Running && !_crashed)
            //    return;

            try {
                _socket.Shutdown(SocketShutdown.Both);
            }
            catch (Exception e) {
                Utils.Error(e.Message);
            }
            try {
                _socket.Close();
            }
            catch(Exception e) { 
                Utils.Error(e.Message);
            }


            _Send.Reset();
            _Receive.Reset();

            _tokenSource.Dispose();
            _tickingTask.Dispose();
            
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

                    //Utils.Log("TcpTicker::Tick");

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
                    BinaryPrimitives.WriteUInt16LittleEndian(_Send.Data.AsSpan()[0..], (ushort)(_Send.PacketLength - LENGTH_PREFIX)); //Length
                        
                    //Debug data
                    //StringBuilder sb = new();
                    //for(int i = 0; i < _Send.PacketLength; i++)
                    //    sb.Append('[').Append(_Send.Data[i]).Append(']');
                    //                
                    //Utils.Log("Sending | Length: {0} Bytes: {1}", _Send.PacketLength, sb.ToString());
                    //End of Debug

                    _ = await _socket.SendAsync(new ArraySegment<byte>(_Send.Data, 0, _Send.PacketLength), SocketFlags.None, _tokenSource.Token);
                }
            }
            catch (Exception e) {
                throw e;
                //Stop(nameof(StartSend));
                //Utils.Error("Send Error {0}", e);
            }
        }

        // called on worker thread
        private static void StartReceive() {
            var len = _socket.Receive(_Receive.PacketBytes, SocketFlags.None);

            if (_tokenSource.IsCancellationRequested) {
                throw new Exception("Cancellation Requested");
            }

            if (len == 0)
                throw new Exception("Data received length is zero");
            
            if (len < LENGTH_PREFIX)
                throw new Exception("Data received length is less then LENGTH_PREFIX");

            //Debug data
            //StringBuilder sb = new();
            //for(int i = 0; i < len; i++)
            //    sb.Append('[').Append(_Receive.PacketBytes[i]).Append(']');
            //                
            //Utils.Log("Received Packet | Length: {0} Bytes: {1}", len, sb.ToString());
            //End of Debug

            ProcessPacket(len);
        }
        private static void ProcessPacket(int len) {
            try {
                int ptr = 0;
                var buffer = _Receive.PacketBytes.AsSpan();

                var packetLen = PacketUtils.ReadUShort(buffer, ref ptr, len);

                //if(len != packetLen + LENGTH_PREFIX) {
                //    if(len >= LENGTH_PREFIX) {
                //        var packetId2 = (S2CPacketId)PacketUtils.ReadByte(buffer[ptr..], ref ptr, len);
                //        Utils.Warn("Packet length miss match {0} != {1}, Ignoring {2}!", len, packetLen + LENGTH_PREFIX, packetId2);
                //    }
                //    else {
                //        Utils.Warn("Packet length miss match {0} != {1}, Ignoring!", len, packetLen + LENGTH_PREFIX);
                //    }
                //
                //    return;
                //}

                //StringBuilder sb = new();
                //for (int i = 0; i < len; i++)
                //    sb.Append('[').Append(_Receive.PacketBytes[i]).Append(']');

                var packetId = (S2CPacketId)PacketUtils.ReadByte(buffer[ptr..], ref ptr, len); //nextPacketPtr
                Utils.Log("Received Packet {1} | Length: {0}", len, packetId);

                PacketHandler.Instance.ReadPacket(packetId, buffer, ref ptr, len); //nextPacketPtr
            }
            catch(Exception e) {
                throw e;
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