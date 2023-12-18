using System;

namespace Networking.Tcp
{
    public interface IOutgoingPacket
    {
        public C2SPacketId Id { get; }
        public void Write(Span<byte> buffer, ref int ptr);
    }
}
