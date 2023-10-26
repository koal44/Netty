using FluentAssertions;
using Netryoshka.Utils;

namespace Tests
{
    public class BinaryPrimitivesTests
    {
        [Fact]
        public void RotateLeft_UInt_Offset8_RotatesAsExpected()
        {
            // Arrange
            uint startValue = 0x1a2b3c4d;
            int offset = 8;

            // Act
            uint rotatedValue = BinaryPrimitives.RotateLeft(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0x2b3c4d1a);
        }

        [Fact]
        public void RotateLeft_UInt_Offset0_RotatesAsExpected()
        {
            // Arrange
            uint startValue = 0x1a2b3c4d;
            int offset = 0;

            // Act
            uint rotatedValue = BinaryPrimitives.RotateLeft(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0x1a2b3c4d);
        }

        [Fact]
        public void RotateLeft_UInt_Offset32_RotatesAsExpected()
        {
            // Arrange
            uint startValue = 0x1a2b3c4d;
            int offset = 32;

            // Act
            uint rotatedValue = BinaryPrimitives.RotateLeft(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0x1a2b3c4d);
        }

        [Fact]
        public void RotateRight_UInt_Offset8_RotatesAsExpected()
        {
            // Arrange
            uint startValue = 0x1a2b3c4d;
            int offset = 8;

            // Act
            uint rotatedValue = BinaryPrimitives.RotateRight(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0x4d1a2b3c);
        }

        [Fact]
        public void RotateLeft_ULong_Offset16_RotatesAsExpected()
        {
            // Arrange
            var startValue = 0x1a2b3c4d5e6f7a8bUL;
            int offset = 16;

            // Act
            var rotatedValue = BinaryPrimitives.RotateLeft(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0x3c4d5e6f7a8b1a2bUL);
        }

        [Fact]
        public void RotateRight_ULong_Offset16_RotatesAsExpected()
        {
            // Arrange
            var startValue = 0x1a2b3c4d5e6f7a8bUL;
            int offset = 16;

            // Act
            var rotatedValue = BinaryPrimitives.RotateRight(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0x7a8b1a2b3c4d5e6fUL);
        }

        [Fact]
        public void RotateRight_UInt_HighBitSet_RotatesAsExpected()
        {
            // Arrange
            uint startValue = 0x80000001;
            int offset = 1;

            // Act
            uint rotatedValue = BinaryPrimitives.RotateRight(startValue, offset);

            // Assert
            rotatedValue.Should().Be(0xC0000000);
        }

        [Fact]
        public void RotateRight_UInt_Offset132_RotatesAsExpected()
        {
            // Arrange
            uint startValue = 0x1a2b3c4d;

            // Act
            uint rotated132Value = BinaryPrimitives.RotateRight(startValue, 132);
            uint rotated4Value = BinaryPrimitives.RotateRight(startValue, 4);

            // Assert
            rotated132Value.Should().Be(rotated4Value);
        }

        [Fact]
        public void ReverseEndianness_Short_ReturnsReversed()
        {
            // Arrange
            short original = 0x1234;
            short expected = 0x3412;

            // Act
            short reversed = BinaryPrimitives.ReverseEndianness(original);

            // Assert
            reversed.Should().Be(expected);
        }

        [Fact]
        public void ReverseEndianness_Int_ReturnsReversed()
        {
            // Arrange
            int original = 0x12345678;
            int expected = 0x78563412;

            // Act
            int reversed = BinaryPrimitives.ReverseEndianness(original);

            // Assert
            reversed.Should().Be(expected);
        }

        [Fact]
        public void ReverseEndianness_Long_ReturnsReversed()
        {
            // Arrange
            long original = 0x0123456789ABCDEF;
            long expected = unchecked((long)0xEFCDAB8967452301);

            // Act
            long reversed = BinaryPrimitives.ReverseEndianness(original);

            // Assert
            reversed.Should().Be(expected);
        }

        [Fact]
        public void ReverseEndianness_SByte_ReturnsSameValue()
        {
            // Arrange
            sbyte original = 0x7F; // 0111 1111

            // Act
            sbyte reversed = BinaryPrimitives.ReverseEndianness(original);

            // Assert
            reversed.Should().Be(original);
        }

        [Fact]
        public void ReverseEndianness_ZeroInt_ReturnsZero()
        {
            // Arrange
            int original = 0;
            int expected = 0;

            // Act
            int reversed = BinaryPrimitives.ReverseEndianness(original);

            // Assert
            reversed.Should().Be(expected);
        }

        [Fact]
        public void ReverseEndianness_Byte_ReturnsSameValue()
        {
            byte value = 0xA5;
            byte reversed = BinaryPrimitives.ReverseEndianness(value);
            reversed.Should().Be(value);
        }

        [Fact]
        public void ReverseEndianness_UShort_ReturnsReversedValue()
        {
            ushort value = 0x1234;
            ushort reversed = BinaryPrimitives.ReverseEndianness(value);
            reversed.Should().Be(0x3412);
        }

        [Fact]
        public void ReverseEndianness_UInt_ReturnsReversedValue()
        {
            uint value = 0x12345678;
            uint reversed = BinaryPrimitives.ReverseEndianness(value);
            reversed.Should().Be(0x78563412);
        }

        [Fact]
        public void ReverseEndianness_ULong_ReturnsReversedValue()
        {
            ulong value = 0x123456789ABCDEF0;
            ulong reversed = BinaryPrimitives.ReverseEndianness(value);
            reversed.Should().Be(0xF0DEBC9A78563412);
        }


    }
}
