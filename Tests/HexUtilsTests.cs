namespace Tests
{
    using FluentAssertions;
    using Netryoshka.Utils;
    using Xunit;

    public class UtilTests
    {
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


    }
}
