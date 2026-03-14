using System;
using System.Collections.Generic;

namespace NeuroKey.Network
{
    public class PacketManager
    {
        public Packet CreatePacket(int id)
        {
            return id switch
            {
                1 => new HandShakePacket(),
                19 => new GenerateQRLoginPacket(),
                20 => new QRLoginResponsePacket(),
                22 => new ChildAuthResponsePacket(),
                _ => throw new Exception("Unknown packet ID: " + id),
            };
        }
    }

    public class HandShakePacket : Packet
    {
        public string HostId;
        public HandShakePacket(string hostId) : base(1) { HostId = hostId; }
        public HandShakePacket() : base(1) { }
        protected override void Write(BinaryWriter writer) { PutString(writer, HostId); }
        protected override void Read(BinaryReader reader) { HostId = ReadString(reader); }
    }

    public class GenerateQRLoginPacket : Packet
    {
        public GenerateQRLoginPacket() : base(19) { }
        protected override void Write(BinaryWriter writer) { }
        protected override void Read(BinaryReader reader) { }
    }

    public class QRLoginResponsePacket : Packet
    {
        public string Token;
        public QRLoginResponsePacket() : base(20) { }
        protected override void Write(BinaryWriter writer) { PutString(writer, Token); }
        protected override void Read(BinaryReader reader) { Token = ReadString(reader); }
    }

    public class ChildAuthResponsePacket : Packet
    {
        public bool Success;
        public long ChildId;
        public string ChildName;
        public ChildAuthResponsePacket() : base(22) { }
        protected override void Write(BinaryWriter writer)
        {
            writer.Write((byte)(Success ? 1 : 0));
            WriteInt32BigEndian(writer, (int)(ChildId >> 32)); // Manual BigEndian Long
            WriteInt32BigEndian(writer, (int)(ChildId & 0xFFFFFFFF));
            PutString(writer, ChildName);
        }
        protected override void Read(BinaryReader reader)
        {
            Success = reader.ReadByte() == 1;
            // Read Long as BigEndian
            byte[] longBytes = reader.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(longBytes);
            ChildId = BitConverter.ToInt64(longBytes, 0);
            ChildName = ReadString(reader);
        }
    }
}
