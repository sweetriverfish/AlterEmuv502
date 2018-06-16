using System;
using System.Text;

namespace Core.Networking
{
    public class InPacket {

        public readonly long Ticks;
        public readonly ushort Id;

        private int currentIndex = 0;

        public string[] Blocks { get; }
        private string packet = "";

        public InPacket(byte[] packetBuffer) {
            packet = Encoding.UTF8.GetString(packetBuffer);

            string[] tempBlocks = packet.Split(' ');

            if (!long.TryParse(tempBlocks[0], out this.Ticks)) {
                throw new Exception("Invalid packet tick.");
            }

            if (!ushort.TryParse(tempBlocks[1], out this.Id)) {
                throw new Exception("Invalid packet id.");
            }
            
            Blocks = new string[tempBlocks.Length - 3];
            Array.Copy(tempBlocks, 2, Blocks, 0, tempBlocks.Length - 3);
        }

        public string ReadString(int index)
        {
            if (index < 0 || index >= Blocks.Length)
                throw new IndexOutOfRangeException();

            return Blocks[index].Replace((char)0x1D, (char)0x20);
        }

        public string ReadString()
        {
            return ReadString(currentIndex++);
        }

        public int ReadInt(int index)
        {
            return int.Parse(ReadString(index));
        }

        public int ReadInt()
        {
            return int.Parse(ReadString());
        }

        public uint ReadUint(int index)
        {
            return uint.Parse(ReadString(index));
        }

        public uint ReadUint()
        {
            return uint.Parse(ReadString());
        }

        public bool ReadBool(int index)
        {
            return ReadString(index) == "1";
        }

        public bool ReadBool()
        {
            return ReadString() == "1";
        }

        public short ReadShort(int index)
        {
            return short.Parse(ReadString(index));
        }

        public short ReadShort()
        {
            return short.Parse(ReadString());
        }

        public ushort ReadUshort(int index)
        {
            return ushort.Parse(ReadString(index));
        }

        public ushort ReadUshort()
        {
            return ushort.Parse(ReadString());
        }

        public byte ReadByte(int index)
        {
            return byte.Parse(ReadString(index));
        }

        public byte ReadByte()
        {
            return byte.Parse(ReadString());
        }

        public sbyte ReadSbyte(int index)
        {
            return sbyte.Parse(ReadString(index));
        }

        public sbyte ReadSbyte()
        {
            return sbyte.Parse(ReadString());
        }

        public override string ToString()
        {
            return packet.Remove(packet.Length - 1);
        }
    }
}
