using FluentAssertions;
using Netryoshka.Utils;
using Xunit;

namespace Tests
{
    public class StringUtilsTests
    {
        [Fact]
        public void StringJoin_JoinsNonEmptyStrings()
        {
            // Arrange
            string[] items = { "apple", "banana", "cherry" };
            string expected = "apple,banana,cherry";

            // Act
            string result = StringUtils.StringJoin(",", items);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void StringJoin_IgnoresNullOrEmptyStrings()
        {
            // Arrange
            string?[] items = { "apple", "", null, "cherry" };
            string expected = "apple,cherry";

            // Act
            string result = StringUtils.StringJoin(",", items);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void StringJoin_WithNullItems_ReturnsEmptyString()
        {
            // Arrange
            string[]? items = null;
            string expected = string.Empty;

            // Act
            string result = StringUtils.StringJoin(",", items);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void StringJoin_WithAllNullOrEmptyItems_ReturnsEmptyString()
        {
            // Arrange
            string?[] items = { "", null, "" };
            string expected = string.Empty;

            // Act
            string result = StringUtils.StringJoin(",", items);

            // Assert
            result.Should().Be(expected);
        }

        [Fact]
        public void StringJoin_WithDifferentSeparator_JoinsCorrectly()
        {
            // Arrange
            string[] items = { "apple", "banana", "cherry" };
            string expected = "apple|banana|cherry";

            // Act
            string result = StringUtils.StringJoin("|", items);

            // Assert
            result.Should().Be(expected);
        }
    }

}
