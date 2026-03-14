using System;
using System.Collections.Generic;
using System.IO;

namespace NeuroKey.Network
{
    public class PacketManager
    {
        public Packet CreatePacket(int id)
        {
            return id switch
            {
                1 => new HandShakePacket(),
                8 => new CompleteTaskPacket(),
                9 => new ActionResponsePacket(),
                11 => new FetchTasksPacket(),
                12 => new FetchTasksResponsePacket(),
                19 => new GenerateQRLoginPacket(),
                20 => new QRLoginResponsePacket(),
                22 => new ChildAuthResponsePacket(),
                23 => new FetchChildStatsPacket(),
                24 => new FetchChildStatsResponsePacket(),
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
            byte[] longBytes = BitConverter.GetBytes(ChildId);
            if (BitConverter.IsLittleEndian) Array.Reverse(longBytes);
            writer.Write(longBytes);
            PutString(writer, ChildName);
        }
        protected override void Read(BinaryReader reader)
        {
            Success = reader.ReadByte() == 1;
            byte[] longBytes = reader.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(longBytes);
            ChildId = BitConverter.ToInt64(longBytes, 0);
            ChildName = ReadString(reader);
        }
    }

    public class CompleteTaskPacket : Packet
    {
        public long ChildId;
        public long TaskId;
        public CompleteTaskPacket(long childId, long taskId) : base(8) { ChildId = childId; TaskId = taskId; }
        public CompleteTaskPacket() : base(8) { }
        protected override void Write(BinaryWriter writer)
        {
            byte[] cBytes = BitConverter.GetBytes(ChildId);
            if (BitConverter.IsLittleEndian) Array.Reverse(cBytes);
            writer.Write(cBytes);
            byte[] tBytes = BitConverter.GetBytes(TaskId);
            if (BitConverter.IsLittleEndian) Array.Reverse(tBytes);
            writer.Write(tBytes);
        }
        protected override void Read(BinaryReader reader)
        {
            byte[] cBytes = reader.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(cBytes);
            ChildId = BitConverter.ToInt64(cBytes, 0);
            byte[] tBytes = reader.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(tBytes);
            TaskId = BitConverter.ToInt64(tBytes, 0);
        }
    }

    public class ActionResponsePacket : Packet
    {
        public int RequestPacketId;
        public bool Success;
        public string Message;
        public long ResultId;
        public ActionResponsePacket() : base(9) { }
        protected override void Write(BinaryWriter writer) { }
        protected override void Read(BinaryReader reader)
        {
            RequestPacketId = ReadInt32BigEndian(reader);
            Success = reader.ReadByte() == 1;
            Message = ReadString(reader);
            byte[] rBytes = reader.ReadBytes(8);
            if (BitConverter.IsLittleEndian) Array.Reverse(rBytes);
            ResultId = BitConverter.ToInt64(rBytes, 0);
        }
    }

    public class FetchTasksPacket : Packet
    {
        public FetchTasksPacket() : base(11) { }
        protected override void Write(BinaryWriter writer) { }
        protected override void Read(BinaryReader reader) { }
    }

    public class FetchTasksResponsePacket : Packet
    {
        public struct TaskDto { public long Id; public string Title; public int Points; }
        public List<TaskDto> Tasks = new List<TaskDto>();
        public FetchTasksResponsePacket() : base(12) { }
        protected override void Write(BinaryWriter writer) { }
        protected override void Read(BinaryReader reader)
        {
            int size = ReadInt32BigEndian(reader);
            for (int i = 0; i < size; i++)
            {
                byte[] idBytes = reader.ReadBytes(8);
                if (BitConverter.IsLittleEndian) Array.Reverse(idBytes);
                long id = BitConverter.ToInt64(idBytes, 0);
                string title = ReadString(reader);
                int points = ReadInt32BigEndian(reader);
                Tasks.Add(new TaskDto { Id = id, Title = title, Points = points });
            }
        }
    }

    public class FetchChildStatsPacket : Packet
    {
        public FetchChildStatsPacket() : base(23) { }
        protected override void Write(BinaryWriter writer) { }
        protected override void Read(BinaryReader reader) { }
    }

    public class FetchChildStatsResponsePacket : Packet
    {
        public string Name;
        public int TotalPoints;
        public string GameStatsJson;
        public FetchChildStatsResponsePacket() : base(24) { }
        protected override void Write(BinaryWriter writer) { }
        protected override void Read(BinaryReader reader)
        {
            Name = ReadString(reader);
            TotalPoints = ReadInt32BigEndian(reader);
            GameStatsJson = ReadString(reader);
        }
    }
}
