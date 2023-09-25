using FluentAssertions;
using Netryoshka.Utils;

namespace Proton.Tests
{
    public class HexConverterTests
    {
        [Theory]
        [InlineData("00FF", new byte[] { 0, 255 })]
        [InlineData("00ff", new byte[] { 0, 255 })]
        [InlineData("1234567890ABCDEF", new byte[] { 18, 52, 86, 120, 144, 171, 205, 239 })]
        [InlineData("", new byte[] { })]
        public void HexToBytes_ShouldConvertCorrectly(string hex, byte[] expected)
        {
            // Act
            byte[] result = Util.HexToBytes(hex);

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
            byte[] result = Util.HexToBytesOptimized(hex);

            // Assert
            result.Should().Equal(expected);
        }

        [Theory]
        [InlineData("F")]
        [InlineData("123")]
        public void HexToBytes_ShouldThrowArgumentException_ForOddLength(string hex)
        {
            // Act
            void act() => Util.HexToBytes(hex);

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
            Action act = () => Util.HexToBytesOptimized(hex);
            act.Should().Throw<ArgumentException>().WithMessage("The hex string cannot have an odd number of digits*");
        }
    }
}
