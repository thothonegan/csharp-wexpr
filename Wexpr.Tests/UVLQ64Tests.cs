using System;
using Xunit;

namespace Wexpr.Tests
{
    public class UVLQ64Tests
    {
        [Fact]
        public void UVLQ64CanEncodeDecode()
        {
            Byte[] tempBuffer = new byte[10];

            UInt64[] x = {
                0x7f, 0x4000, 0, 0x3ffffe, 0x1fffff, 0x200000, 0x3311a1234df31413
            };

            foreach (UInt64 v in x)
            {
                bool writeResult = UVLQ64.Write(tempBuffer, v);
                Assert.True(writeResult);

                UInt64 outResult;
                Int64 readResult = UVLQ64.Read(tempBuffer, out outResult);
                Assert.NotEqual(-1, readResult);
                Assert.Equal (v, outResult);
            }
        }
    }
}
