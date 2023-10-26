using Xunit;
using FluentAssertions;
using Netryoshka.Utils;

namespace Tests
{
    public class BitUtilsTests
    {
        [Theory]
        [InlineData(0x1234567890ABCDEF, Endianness.Big, new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF })]
        [InlineData(0x1234567890ABCDEF, Endianness.Little, new byte[] { 0xEF, 0xCD, 0xAB, 0x90, 0x78, 0x56, 0x34, 0x12 })]
        public void CopyBytes_WithLong_ShouldCopyIntoBuffer(long value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[8];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(0x1234, Endianness.Big, new byte[] { 0x12, 0x34 })]
        [InlineData(0x1234, Endianness.Little, new byte[] { 0x34, 0x12 })]
        public void CopyBytes_WithShort_ShouldCopyIntoBuffer(short value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[2];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(true, new byte[] { 1 })]
        [InlineData(false, new byte[] { 0 })]
        public void CopyBytes_WithBool_ShouldCopyIntoBuffer(bool value, byte[] expected)
        {
            var buffer = new byte[1];
            BitUtils.CopyBytes(value, buffer, 0);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(1.0, Endianness.Big, new byte[] { 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(1.0, Endianness.Little, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F })]
        public void CopyBytes_WithDouble_ShouldCopyIntoBuffer(double value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[8];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(0x12345678, Endianness.Big, new byte[] { 0x12, 0x34, 0x56, 0x78 })]
        [InlineData(0x12345678, Endianness.Little, new byte[] { 0x78, 0x56, 0x34, 0x12 })]
        public void CopyBytes_WithInt_ShouldCopyIntoBuffer(int value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[4];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(0x1234, Endianness.Big, new byte[] { 0x12, 0x34 })]
        [InlineData(0x1234, Endianness.Little, new byte[] { 0x34, 0x12 })]
        public void CopyBytes_WithUShort_ShouldCopyIntoBuffer(ushort value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[2];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(0x12345678u, Endianness.Big, new byte[] { 0x12, 0x34, 0x56, 0x78 })]
        [InlineData(0x12345678u, Endianness.Little, new byte[] { 0x78, 0x56, 0x34, 0x12 })]
        public void CopyBytes_WithUInt_ShouldCopyIntoBuffer(uint value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[4];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(0x1234567890ABCDEFul, Endianness.Big, new byte[] { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF })]
        [InlineData(0x1234567890ABCDEFul, Endianness.Little, new byte[] { 0xEF, 0xCD, 0xAB, 0x90, 0x78, 0x56, 0x34, 0x12 })]
        public void CopyBytes_WithULong_ShouldCopyIntoBuffer(ulong value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[8];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(1.0f, Endianness.Big, new byte[] { 0x3F, 0x80, 0x00, 0x00 })]
        [InlineData(1.0f, Endianness.Little, new byte[] { 0x00, 0x00, 0x80, 0x3F })]
        public void CopyBytes_WithFloat_ShouldCopyIntoBuffer(float value, Endianness endianness, byte[] expected)
        {
            var buffer = new byte[4];
            BitUtils.CopyBytes(value, buffer, 0, endianness);
            buffer.Should().Equal(expected);
        }

        [Theory]
        [InlineData(123.45d, Endianness.Big, new byte[] { 0x00, 0x00, 0x30, 0x39, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00 })]
        [InlineData(123.45d, Endianness.Little, new byte[] { 0x39, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00 })]
        public void GetBytes_WithDecimal_ShouldReturnByteArray(decimal value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Fact]
        public void GetBytes_WithBool_ShouldReturnByteArray()
        {
            var result = BitUtils.GetBytes(true);
            result.Should().Equal(new byte[] { 1 });
        }

        [Theory]
        [InlineData('A', Endianness.Big, new byte[] { 0x00, 0x41 })]
        [InlineData('A', Endianness.Little, new byte[] { 0x41, 0x00 })]
        public void GetBytes_WithChar_ShouldReturnByteArray(char value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData((ushort)1, Endianness.Big, new byte[] { 0x00, 0x01 })]
        [InlineData((ushort)1, Endianness.Little, new byte[] { 0x01, 0x00 })]
        public void GetBytes_WithUShort_ShouldReturnByteArray(ushort value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData((uint)1, Endianness.Big, new byte[] { 0x00, 0x00, 0x00, 0x01 })]
        [InlineData((uint)1, Endianness.Little, new byte[] { 0x01, 0x00, 0x00, 0x00 })]
        public void GetBytes_WithUInt_ShouldReturnByteArray(uint value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData((ulong)1, Endianness.Big, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 })]
        [InlineData((ulong)1, Endianness.Little, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
        public void GetBytes_WithULong_ShouldReturnByteArray(ulong value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }


        [Theory]
        [InlineData(1.0, Endianness.Big, new byte[] { 0x3F, 0xF0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(1.0, Endianness.Little, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xF0, 0x3F })]
        public void GetBytes_WithDouble_ShouldReturnByteArray(double value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData((short)1, Endianness.Big, new byte[] { 0x00, 0x01 })]
        [InlineData((short)1, Endianness.Little, new byte[] { 0x01, 0x00 })]
        public void GetBytes_WithShort_ShouldReturnByteArray(short value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData(1, Endianness.Big, new byte[] { 0x00, 0x00, 0x00, 0x01 })]
        [InlineData(1, Endianness.Little, new byte[] { 0x01, 0x00, 0x00, 0x00 })]
        public void GetBytes_WithInt_ShouldReturnByteArray(int value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData((long)1, Endianness.Big, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01 })]
        [InlineData((long)1, Endianness.Little, new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]
        public void GetBytes_WithLong_ShouldReturnByteArray(long value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData(1.0f, Endianness.Big, new byte[] { 0x3F, 0x80, 0x00, 0x00 })]
        [InlineData(1.0f, Endianness.Little, new byte[] { 0x00, 0x00, 0x80, 0x3F })]
        public void GetBytes_WithFloat_ShouldReturnByteArray(float value, Endianness endianness, byte[] expected)
        {
            var result = BitUtils.GetBytes(value, endianness);
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData(1.0, 4607182418800017408L)]
        [InlineData(-1.0, -4616189618054758400L)]
        public void DoubleToInt64Bits_ShouldReturnCorrectValue(double input, long expected)
        {
            long result = BitUtils.DoubleToInt64Bits(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(4607182418800017408L, 1.0)]
        [InlineData(-4616189618054758400L, -1.0)]
        public void Int64BitsToDouble_ShouldReturnCorrectValue(long input, double expected)
        {
            double result = BitUtils.Int64BitsToDouble(input);
            result.Should().BeApproximately(expected, 1e-10);
        }

        [Theory]
        [InlineData(1.0f, 1065353216)]
        [InlineData(-1.0f, -1082130432)]
        public void SingleToInt32Bits_ShouldReturnCorrectValue(float input, int expected)
        {
            int result = BitUtils.SingleToInt32Bits(input);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(1065353216, 1.0f)]
        [InlineData(-1082130432, -1.0f)]
        public void Int32BitsToSingle_ShouldReturnCorrectValue(int input, float expected)
        {
            float result = BitUtils.Int32BitsToSingle(input);
            result.Should().BeApproximately(expected, 1e-6f);
        }


        [Fact]
        public void ToBoolean_ShouldReturnCorrectValue()
        {
            byte[] input = { 0, 1, 0 };
            bool result = BitUtils.ToBoolean(input, 1);
            result.Should().Be(true);
        }

        [Fact]
        public void ToChar_ShouldReturnCorrectValue()
        {
            byte[] inputBigEndian = { 0x00, 0x41 };
            byte[] inputLittleEndian = { 0x41, 0x00 };

            char resultBig = BitUtils.ToChar(inputBigEndian, 0, Endianness.Big);
            char resultLittle = BitUtils.ToChar(inputLittleEndian, 0, Endianness.Little);

            resultBig.Should().Be('A');
            resultLittle.Should().Be('A');
        }

        [Fact]
        public void ToDouble_ShouldReturnCorrectValue()
        {
            byte[] input = BitConverter.GetBytes(123.45);
            double result = BitUtils.ToDouble(input, 0, Endianness.Little);
            result.Should().BeApproximately(123.45, 1e-9);
        }

        [Fact]
        public void ToSingle_ShouldReturnCorrectValue()
        {
            byte[] input = BitConverter.GetBytes(123.45f);
            float result = BitUtils.ToSingle(input, 0, Endianness.Little);
            ((double)result).Should().BeApproximately(123.45, 1e-5);
        }

        [Fact]
        public void ToString_ShouldReturnCorrectValue()
        {
            byte[] input = { 0x01, 0x02, 0x03 };
            string result = BitUtils.ToString(input);
            result.Should().Be("01-02-03");
        }

        [Fact]
        public void ToString_WithStartIndex_ShouldReturnCorrectValue()
        {
            byte[] input = { 0x01, 0x02, 0x03 };
            string result = BitUtils.ToString(input, 1);
            result.Should().Be("02-03");
        }

        [Fact]
        public void ToString_WithStartIndexAndLength_ShouldReturnCorrectValue()
        {
            byte[] input = { 0x01, 0x02, 0x03, 0x04 };
            string result = BitUtils.ToString(input, 1, 2);
            result.Should().Be("02-03");
        }

        [Theory]
        [InlineData(new byte[] { 0x12, 0x34 }, 0, Endianness.Big, 0x1234)]
        [InlineData(new byte[] { 0x12, 0x34 }, 0, Endianness.Little, 0x3412)]
        [InlineData(new byte[] { 0xFF, 0x12, 0x34 }, 1, Endianness.Big, 0x1234)]
        public void ToUInt16_WithVariousConditions_ShouldReturnCorrectValue(byte[] buffer, int startIndex, Endianness endianness, ushort expected)
        {
            ushort result = BitUtils.ToUInt16(buffer, startIndex, endianness);
            result.Should().Be(expected);
        }


        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFE }, 0, Endianness.Big, -2)]
        [InlineData(new byte[] { 0xFE, 0xFF }, 0, Endianness.Little, -2)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xFE }, 1, Endianness.Big, -2)]
        public void ToInt16_WithVariousConditions_ShouldReturnCorrectValue(byte[] buffer, int startIndex, Endianness endianness, short expected)
        {
            short result = BitUtils.ToInt16(buffer, startIndex, endianness);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(new byte[] { 0x12, 0x34, 0x56, 0x78 }, 0, Endianness.Big, 0x12345678U)]
        [InlineData(new byte[] { 0x12, 0x34, 0x56, 0x78 }, 0, Endianness.Little, 0x78563412U)]
        [InlineData(new byte[] { 0xFF, 0x12, 0x34, 0x56, 0x78 }, 1, Endianness.Big, 0x12345678U)]
        public void ToUInt32_WithVariousConditions_ShouldReturnCorrectValue(byte[] buffer, int startIndex, Endianness endianness, uint expected)
        {
            uint result = BitUtils.ToUInt32(buffer, startIndex, endianness);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFE }, 0, Endianness.Big, -2)]
        [InlineData(new byte[] { 0xFE, 0xFF, 0xFF, 0xFF }, 0, Endianness.Little, -2)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }, 1, Endianness.Big, -2)]
        public void ToInt32_WithVariousConditions_ShouldReturnCorrectValue(byte[] buffer, int startIndex, Endianness endianness, int expected)
        {
            int result = BitUtils.ToInt32(buffer, startIndex, endianness);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }, 0, Endianness.Big, 0x123456789ABCDEF0UL)]
        [InlineData(new byte[] { 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }, 0, Endianness.Little, 0xF0DEBC9A78563412UL)]
        [InlineData(new byte[] { 0xFF, 0x12, 0x34, 0x56, 0x78, 0x9A, 0xBC, 0xDE, 0xF0 }, 1, Endianness.Big, 0x123456789ABCDEF0UL)]
        public void ToUInt64_WithVariousConditions_ShouldReturnCorrectValue(byte[] buffer, int startIndex, Endianness endianness, ulong expected)
        {
            ulong result = BitUtils.ToUInt64(buffer, startIndex, endianness);
            result.Should().Be(expected);
        }

        [Theory]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }, 0, Endianness.Big, -2L)]
        [InlineData(new byte[] { 0xFE, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF }, 0, Endianness.Little, -2L)]
        [InlineData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE }, 1, Endianness.Big, -2L)]
        public void ToInt64_WithVariousConditions_ShouldReturnCorrectValue(byte[] buffer, int startIndex, Endianness endianness, long expected)
        {
            long result = BitUtils.ToInt64(buffer, startIndex, endianness);
            result.Should().Be(expected);
        }

    }

}
