using System;
using System.Buffers.Binary;
using System.Text;
using UnityEngine.Assertions;

namespace Networking.Tcp {
    public static class PacketUtils {
        public static void WriteByte(Span<byte> buffer, byte value, ref int ptr) {
            Utils.Log("Trying to write byte {0} at {1}", value, ptr);

            if (ptr + 1 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 1, buffer.Length);
                return;
            }

            buffer[0] = value;
            Utils.Log("Wrote byte {0} at {1} | Validation {2}", value, ptr, buffer[0] == value);
            ptr++;
        }
        public static void WriteChar(Span<byte> buffer, char value, ref int ptr) {
            Utils.Log("Trying to write char {0} at {1}", value, ptr);
            WriteByte(buffer[ptr..], Convert.ToByte(value), ref ptr);
        }
        public static void WriteBool(Span<byte> buffer, bool value, ref int ptr) {
            Utils.Log("Trying to write bool {0} at {1}", value, ptr);
            WriteByte(buffer[ptr..], Convert.ToByte(value), ref ptr);
        }
        public static void WriteShort(Span<byte> buffer, short value, ref int ptr) {
            Utils.Log("Trying to write short {0} at {1}", value, ptr);
            if (ptr + 2 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 2, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteInt16LittleEndian(buffer[ptr..], value);
            ptr += 2;
        }
        public static void WriteUShort(Span<byte> buffer, ushort value, ref int ptr) {
            Utils.Log("Trying to write ushort {0} at {1}", value, ptr);
            if (ptr + 2 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 2, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteUInt16LittleEndian(buffer[ptr..], value);
            ptr += 2;
        }
        public static void WriteInt(Span<byte> buffer, int value, ref int ptr) {
            Utils.Log("Trying to write int {0}, {1}", value, ptr);
            if (ptr + 4 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 4, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteInt32LittleEndian(buffer[ptr..], value);
            ptr += 4;
        }
        public static void WriteUInt(Span<byte> buffer, uint value, ref int ptr) {
            Utils.Log("Trying to write uint {0} at {1}", value, ptr);
            if (ptr + 4 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 4, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteUInt32LittleEndian(buffer[ptr..], value);
            ptr += 4;
        }
        public static void WriteLong(Span<byte> buffer, long value, ref int ptr) {
            Utils.Log("Trying to write long {0} at {1}", value, ptr);
            if (ptr + 8 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 8, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteInt64LittleEndian(buffer[ptr..], value);
            ptr += 8;
        }
        public static void WriteULong(Span<byte> buffer, ulong value, ref int ptr) {
            Utils.Log("Trying to write ulong {0} at {1}", value, ptr);
            if (ptr + 8 > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 8, buffer.Length);
                return;
            }

            BinaryPrimitives.WriteUInt64LittleEndian(buffer[ptr..], value);
            ptr += 8;
        }
        public static void WriteString(Span<byte> buffer, string value, ref int ptr) {
            Utils.Log("Trying to write string {0} length: {1}", value, value.Length);
            if (ptr + 2 + (ushort)value.Length > buffer.Length) {
                Utils.Error("Send buffer attempted to write out of bounds {0}, {1}", ptr + 2, buffer.Length);
                return;
            }

            Utils.Log("Trying to write string length {0} at {1}", value.Length, ptr);
            WriteUShort(buffer, (ushort)value.Length, ref ptr);

            foreach(var b in value) 
                WriteChar(buffer, b, ref ptr);
        }
    }
}