using System;

namespace Wexpr
{
    //
    /// <summary>
    /// Support functions for UVLQ64 values
    /// </summary>
    //
    public static class UVLQ64
    {
        //
        /// <summary>
        /// Return the number of bytes which is needed to store a value in the UVLQ64.
        /// </summary>
        /// <param name="value">The value to test</param>
        /// <returns>The number of bytes needed to store value in UVLQ64.</returns>
        //
        public static int ByteSizeFor (UInt64 value)
        {
            // we get 7 bits per byte. 2^7 for each.
            if (value < 128)       return 1; // 2^7
            if (value < 16384)     return 2; // 2^14
            if (value < 2097152)   return 3; // 2^21
            if (value < 268435456) return 4; // 2^28
            if (value < 34359738368)         return 5; // 2^35
            if (value < 4398046511104)       return 6; // 2^42
            if (value < 562949953421312)     return 7; // 2^49
            if (value < 72057594037927936)   return 8; // 2^56
            if (value < 9223372036854775808) return 9; // 2^63
            return 10; // 2^64+
        }

        //
        /// <summary>
        /// Write a UVLQ64 (big endian) to the given buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="value">The value to write</param>
        /// <returns>true on sucess, false on failure (invalid buffer).</returns>
        //
        public static bool Write (Byte[] buffer, UInt64 value)
        {
            int bytesNeeded = ByteSizeFor(value);
            if (buffer.Length < bytesNeeded)
                return false; // Not big enough buffer

            int i = bytesNeeded - 1;
            for (int j = 0; j <= i; ++j)
                buffer[j] = Convert.ToByte(((value >> ((i - j) * 7)) & 127) | 128);

            buffer[i] ^= 128;
            return true;
        }

        //
        /// <summary>
        /// Read a UVLQ64 (big endian) from teh given buffer.
        /// </summary>
        /// <param name="buffer">The buffer to read from</param>
        /// <param name="outValue">The place to store the value read</param>
        /// <returns>The index to the first byte in the buffer we didnt read, or -1 if failure.</returns>
        //
        public static Int64 Read (Byte[] buffer, out UInt64 outValue)
        {
            UInt64 r = 0;
            outValue = 0;

            int bufferSize = buffer.Length;
            int i = 0;

            do
            {
                if (bufferSize == 0) return -1;

                r = (r << 7) | Convert.ToUInt64(buffer[i] & 127);
                --bufferSize;
                ++i;
            } while ((buffer[i-1] & 128) != 0);

            outValue = r;
            return i;
        }
    }
}
