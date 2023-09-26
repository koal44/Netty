using FluentAssertions;
using Netryoshka.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Tests
{
    public class JsonHelperTests
    {
        [Fact]
        public void DeserializeAndCombineDuplicateKeys_ShouldCombineDuplicateKeys()
        {
            // Arrange
            var json = @"{
                ""key"": ""value1"",
                ""key"": ""value2""
            }";
            using var reader = new JsonTextReader(new StringReader(json));

            // Act
            var result = JsonUtil.DeserializeAndCombineDuplicateKeys(reader);

            // Assert
            result.Should().NotBeNull();
            var objResult = result as JObject;
            objResult.Should().NotBeNull();

            var property = objResult!.Property("key");
            property.Should().NotBeNull();

            var value = property!.Value;
            value.Type.Should().Be(JTokenType.Array);

            var array = value as JArray;
            array.Should().NotBeNull();
            array!.Count.Should().Be(2);
            array![0].Value<string>().Should().Be("value1");
            array![1].Value<string>().Should().Be("value2");
        }

        [Fact]
        public void DeserializeAndCombineDuplicateKeys_ShouldCombineDuplicateIntKeys()
        {
            // Arrange
            var json = @"{
                ""Foo"": 1,
                ""Foo"": 2
            }";
            using var reader = new JsonTextReader(new StringReader(json));

            // Act
            var result = JsonUtil.DeserializeAndCombineDuplicateKeys(reader);

            // Assert
            var jObject = result.Should().BeOfType<JObject>().Subject;

            jObject.Should().ContainKey("Foo");
            var fooValue = jObject["Foo"];

            var jArray = fooValue.Should().BeOfType<JArray>().Subject;
            jArray.Should().HaveCount(2);

            ((int)jArray[0]).Should().Be(1);
            ((int)jArray[1]).Should().Be(2);
        }

        [Fact]
        public void DeserializeAndCombineDuplicateKeys_ShouldCombineDuplicateNestedKeys()
        {
            // Arrange
            var json = @"{
                ""Bar"": { ""X"": ""A"" },
                ""Bar"": { ""X"": ""B"" }
            }";
            using var reader = new JsonTextReader(new StringReader(json));

            // Act
            var result = JsonUtil.DeserializeAndCombineDuplicateKeys(reader);

            // Assert
            result.Should().BeOfType<JObject>();
            var jObject = (JObject)result;
            jObject.Should().ContainKey("Bar");
            jObject["Bar"]!.Type.Should().Be(JTokenType.Array);
            jObject["Bar"]![0]!["X"]!.Value<string>().Should().Be("A");
            jObject["Bar"]![1]!["X"]!.Value<string>().Should().Be("B");
        }

        [Fact]
        public void DeserializeAndCombineDuplicateKeys_ShouldCombineDuplicateNestedKeys2()
        {
            // Arrange
            var json = @"{
                ""Bar"": { ""X"": ""A"" },
                ""Bar"": { ""X"": ""B"" }
            }";
            using var reader = new JsonTextReader(new StringReader(json));

            // Act
            var result = JsonUtil.DeserializeAndCombineDuplicateKeys(reader);

            // Assert
            var jObject = (JObject)result;
            var expectedJObject = JObject.Parse(@"{
                ""Bar"": [
                    { ""X"": ""A"" },
                    { ""X"": ""B"" }
                ]
            }");

            var actualDict = jObject.ToObject<Dictionary<string, object>>();
            var expectedDict = expectedJObject.ToObject<Dictionary<string, object>>();

            actualDict.Should().BeEquivalentTo(expectedDict);
        }


        [Fact]
        public void DeserializeAndCombineDuplicateKeys_ShouldHandleComplexCase()
        {
            // Arrange
            var json = @"{
                ""Foo"": 1,
                ""Foo"": [2],
                ""Foo"": [3, 4],
                ""Bar"": { ""X"": [""A"", ""B""] },
                ""Bar"": { ""X"": ""C"", ""X"": ""D"" }
            }";
            using var reader = new JsonTextReader(new StringReader(json));

            // Act
            var result = JsonUtil.DeserializeAndCombineDuplicateKeys(reader);

            // Assert
            var jObject = (JObject)result;
            jObject.Should().BeOfType<JObject>();

            var expectedJObject = JObject.Parse(@"{
                ""Foo"": [1, 2, 3, 4],
                ""Bar"": [
                    { ""X"": [""A"", ""B""] },
                    { ""X"": [""C"", ""D""] }
                ]
            }");

            var actualDict = jObject.ToObject<Dictionary<string, object>>();
            var expectedDict = expectedJObject.ToObject<Dictionary<string, object>>();

            actualDict.Should().BeEquivalentTo(expectedDict);
        }


    }



}
