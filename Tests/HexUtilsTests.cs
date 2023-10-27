using FluentAssertions;
using Netryoshka.Utils;
using Xunit;

namespace Tests
{
    public class UtilTests
    {

        [Theory]
        [InlineData("00FF", new byte[] { 0, 255 })]
        [InlineData("00ff", new byte[] { 0, 255 })]
        [InlineData("1234567890ABCDEF", new byte[] { 18, 52, 86, 120, 144, 171, 205, 239 })]
        [InlineData("", new byte[] { })]
        public void HexToBytes_ShouldConvertCorrectly(string hex, byte[] expected)
        {
            // Act
            byte[] result = HexUtils.HexToBytes(hex);

            // Assert
            result.Should().Equal(expected);
        }


        [Theory]
        [InlineData("00FF", new byte[] { 0, 255 })]
        [InlineData("00ff", new byte[] { 0, 255 })]
        [InlineData("1234567890ABCDEF", new byte[] { 18, 52, 86, 120, 144, 171, 205, 239 })]
        [InlineData("", new byte[] { })]
        public void HexToBytesOptimized_ShouldConvertCorrectly(string hex, byte[] expected)
        {
            // Act
            byte[] result = HexUtils.HexToBytesOptimized(hex);

            // Assert
            result.Should().Equal(expected);
        }


        [Theory]
        [InlineData("F")]
        [InlineData("123")]
        public void HexToBytes_ShouldThrowArgumentException_ForOddLength(string hex)
        {
            // Act
            void act() => HexUtils.HexToBytes(hex);

            // Assert
            var exception = Record.Exception(act);
            exception.Should().NotBeNull().And.BeOfType<ArgumentException>();
            exception.Message.Should().StartWith("The hex string cannot have an odd number of digits");
        }


        [Theory]
        [InlineData("F")]
        [InlineData("123")]
        public void HexToBytesOptimized_ShouldThrowArgumentException_ForOddLength(string hex)
        {
            // Act & Assert
            Action act = () => HexUtils.HexToBytesOptimized(hex);
            act.Should().Throw<ArgumentException>().WithMessage("The hex string cannot have an odd number of digits*");
        }


        [Theory]
        [InlineData("DEADBEEF", 2, 0, Endianness.Big, true, 0xDEAD)]
        [InlineData("DEADBEEF", 2, 1, Endianness.Big, true, 0xADBE)]
        [InlineData("DEADBEEF", 4, 0, Endianness.Big, true, 3735928559)]
        [InlineData("DEADBEEF", 2, 0, Endianness.Little, true, 0xADDE)]
        public void TryReadNBytesFromHex_ValidInputs(string hex, int nBytes, int byteOffset, Endianness endianness, bool expectedResult, uint expectedValue)
        {
            // Act
            var result = HexUtils.TryReadNBytesFromHex(hex, nBytes, byteOffset, endianness, out uint value);

            // Assert
            result.Should().Be(expectedResult);
            value.Should().Be(expectedValue);
        }


        [Theory]
        //[InlineData("DEADBEE", 2, 0)]  // Odd-length hex string
        [InlineData("DEADBEEF", 5, 0)] // Requesting more than 4 bytes
        [InlineData("DEADBEEF", 0, 0)] // Requesting 0 bytes
        [InlineData("DEADBEEF", 2, 3)] // Offset too large
        public void TryReadNBytesFromHex_InvalidInputs(string hex, int nBytes, int byteOffset)
        {
            // Act
            var result = HexUtils.TryReadNBytesFromHex(hex, nBytes, byteOffset, Endianness.Big, out uint _);

            // Assert
            result.Should().BeFalse();
        }


        [Theory]
        [InlineData(0x0, TextCase.Upper, '0')]
        [InlineData(0x1, TextCase.Upper, '1')]
        [InlineData(0x9, TextCase.Upper, '9')]
        [InlineData(0xA, TextCase.Upper, 'A')]
        [InlineData(0xF, TextCase.Upper, 'F')]
        [InlineData(0xA, TextCase.Lower, 'a')]
        [InlineData(0xF, TextCase.Lower, 'f')]
        [InlineData(0x1F, TextCase.Lower, 'f')]
        [InlineData(0x12F, TextCase.Lower, 'f')]
        [InlineData(0x123F, TextCase.Lower, 'f')]
        [InlineData(0x1234567F, TextCase.Lower, 'f')]
        public void NibbleToHexChar_ShouldReturnCorrectValue(int input, TextCase textCase, char expected)
        {
            // Act
            var result = HexUtils.NibbleToHexChar(input, textCase);

            // Assert
            result.Should().Be(expected);
        }


        [Theory]
        [InlineData(new byte[] { 0x1, 0x2 }, TextCase.Upper, null, "0102")]
        [InlineData(new byte[] { 0x1, 0x2, 0xA, 0xF }, TextCase.Upper, null, "01020A0F")]
        [InlineData(new byte[] { 0x1, 0x2, 0xA, 0xF }, TextCase.Lower, null, "01020a0f")]
        [InlineData(new byte[] { 0x1, 0x2, 0xA, 0xF }, TextCase.Upper, '-', "01-02-0A-0F")]
        [InlineData(new byte[] { 0x1, 0x2, 0xA, 0xF }, TextCase.Lower, ':', "01:02:0a:0f")]
        public void BytesToHex_ShouldReturnCorrectValue(byte[] bytes, TextCase textCase, char? separator, string expected)
        {
            // Act
            var result = HexUtils.BytesToHex(bytes, textCase, separator);

            // Assert
            result.Should().Be(expected);
        }

    }
}
