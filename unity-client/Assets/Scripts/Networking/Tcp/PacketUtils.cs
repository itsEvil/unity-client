using System;
using System.Buffers.Binary;
using System.Text;
using UnityEngine;
using UnityEngine.Assertions;

namespace Networking.Tcp {
    public static class PacketUtils {
        public static void WriteByte(Span<byte> buffer, byte value, ref int ptr) {
            //Utils.Log("Trying to write byte {0} at {1}", value, ptr);

            if (ptr + 1 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 1, buffer.Length);
                return;
            }

            buffer[0] = value;
            //Utils.Log("Wrote byte {0} at {1} | Validation {2}", value, ptr, buffer[0] == value);
            ptr++;
        }
        public static void WriteChar(Span<byte> buffer, char value, ref int ptr) {
            //Utils.Log("Trying to write char {0} at {1}", value, ptr);
            WriteByte(buffer[ptr..], Convert.ToByte(value), ref ptr);
        }
        public static void WriteBool(Span<byte> buffer, bool value, ref int ptr) {
            //Utils.Log("Trying to write bool {0} at {1}", value, ptr);
            WriteByte(buffer[ptr..], Convert.ToByte(value), ref ptr);
        }
        public static void WriteShort(Span<byte> buffer, short value, ref int ptr) {
            //Utils.Log("Trying to write short {0} at {1}", value, ptr);
            if (ptr + 2 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 2, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteInt16LittleEndian(buffer[ptr..], value);
            ptr += 2;
        }
        public static void WriteUShort(Span<byte> buffer, ushort value, ref int ptr) {
            //Utils.Log("Trying to write ushort {0} at {1}", value, ptr);
            if (ptr + 2 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 2, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteUInt16LittleEndian(buffer[ptr..], value);
            ptr += 2;
        }
        public static void WriteInt(Span<byte> buffer, int value, ref int ptr) {
            //Utils.Log("Trying to write int {0}, {1}", value, ptr);
            if (ptr + 4 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 4, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteInt32LittleEndian(buffer[ptr..], value);
            ptr += 4;
        }
        public static void WriteFloat(Span<byte> buffer, float value, ref int ptr) {
            if (ptr + 4 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 4, buffer.Length);
                return;
            }

            var bytes = BitConverter.GetBytes(value);
            bytes.CopyTo(buffer[ptr..]);
            ptr += 4;
        }
        public static void WriteUInt(Span<byte> buffer, uint value, ref int ptr) {
            //Utils.Log("Trying to write uint {0} at {1}", value, ptr);
            if (ptr + 4 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 4, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteUInt32LittleEndian(buffer[ptr..], value);
            ptr += 4;
        }
        public static void WriteLong(Span<byte> buffer, long value, ref int ptr) {
            //Utils.Log("Trying to write long {0} at {1}", value, ptr);
            if (ptr + 8 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 8, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteInt64LittleEndian(buffer[ptr..], value);
            ptr += 8;
        }
        public static void WriteULong(Span<byte> buffer, ulong value, ref int ptr) {
            //Utils.Log("Trying to write ulong {0} at {1}", value, ptr);
            if (ptr + 8 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 8, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteUInt64LittleEndian(buffer[ptr..], value);
            ptr += 8;
        }
        public static void WriteString(Span<byte> buffer, string value, ref int ptr) {
            //Utils.Log("Trying to write string {0} length: {1}", value, value.Length);
            if (ptr + 2 + (ushort)value.Length > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 2, buffer.Length);
                return;
            }

            //Utils.Log("Trying to write string length {0} at {1}", value.Length, ptr);
            WriteUShort(buffer, (ushort)value.Length, ref ptr);

            foreach(var b in value) 
                WriteChar(buffer, b, ref ptr);
        }

        public static byte ReadByte(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read byte at {0}", ptr);

            if (ptr + 1 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 1}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 1, len);
                //return 0;
            }
            var data = buffer[0];
            ptr++;
            return data;
        }
        public static char ReadChar(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read char at {0}", ptr);
            var data = ReadByte(buffer[ptr..], ref ptr, len);
            return Convert.ToChar(data);
        }
        public static bool ReadBool(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read bool at {0}", ptr);
            var data = ReadByte(buffer[ptr..], ref ptr, len);
            return Convert.ToBoolean(data);
        }
        public static short ReadShort(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read short at {0}", ptr);

            if (ptr + 2 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 2}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 2, len);
                //return 0;
            }

            var data = BinaryPrimitives.ReadInt16LittleEndian(buffer[ptr..]);
            ptr += 2;
            return data;
        }
        public static ushort ReadUShort(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read ushort at {0}", ptr);

            if (ptr + 2 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 2}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 2, len);
                //return 0;
            }

            var data = BinaryPrimitives.ReadUInt16LittleEndian(buffer[ptr..]);
            ptr += 2;
            return data;
        }
        public static int ReadInt(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read int at {0}, {1}", ptr, len);

            if (ptr + 4 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 4}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 4, len);
                //return 0;
            }

            var data = BinaryPrimitives.ReadInt32LittleEndian(buffer[ptr..]);
            ptr += 4;
            return data;
        }
        public static uint ReadUInt(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read uint at {0}", ptr);

            if (ptr + 4 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 4}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 4, len);
                //return 0;
            }

            var data = BinaryPrimitives.ReadUInt32LittleEndian(buffer[ptr..]);
            ptr += 4;
            return data;
        }
        public static float ReadFloat(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read float at {0}, {1}", ptr, len);  
            if (ptr + 4 > len) 
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 4}, {len}");

            float data = BitConverter.ToSingle(buffer[ptr..(ptr + 4)]);
            ptr += 4;
            return data;
        }
        public static long ReadLong(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read long at {0}", ptr);

            if (ptr + 8 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 8}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 8, len);
                //return 0;
            }

            var data = BinaryPrimitives.ReadInt64LittleEndian(buffer[ptr..]);
            ptr += 8;
            return data;
        }
        public static ulong ReadULong(Span<byte> buffer, ref int ptr, int len) {
            //Utils.Log("Trying to read ulong at {0}", ptr);

            if (ptr + 8 > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + 8}, {len}");
                //Utils.Error("Receive buffer attempted to read out of bounds {0}, {1}", ptr + 8, len);
                //return 0;
            }

            var data = BinaryPrimitives.ReadUInt64LittleEndian(buffer[ptr..]);
            ptr += 8;
            return data;
        }
        public static string ReadString(Span<byte> buffer, ref int ptr, int len) {
            ushort strLen = ReadUShort(buffer, ref ptr, len);

            if(ptr + strLen > len) {
                throw new Exception($"Receive buffer attempted to read out of bounds {ptr + strLen}, {len}");
            }
            Span<byte> bytes = stackalloc byte[strLen];
            buffer[ptr..(ptr + strLen)].CopyTo(bytes);
            ptr += strLen;
            var data = Encoding.ASCII.GetString(bytes);

            //Utils.Log("Read string '{0}'", data);
            return data;
        }
    }
}