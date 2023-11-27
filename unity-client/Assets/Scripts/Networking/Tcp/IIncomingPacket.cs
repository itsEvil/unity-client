namespace Networking.Tcp
{
    public interface IIncomingPacket {
        public S2CPacketId Id { get; }
        public void Handle();
    }
}
