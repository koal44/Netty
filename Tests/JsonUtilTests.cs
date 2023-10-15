using FluentAssertions;
using Netryoshka.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Netryoshka.Utils.JsonUtil;

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

    public class IntToListConverterTests
    {
        public class TestClass
        {
            [JsonProperty("property3")]
            [JsonConverter(typeof(IntToListConverter))]
            public List<int>? Property3List { get; set; }
        }

        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new IntToListConverter() }
        };

        [Fact]
        public void Deserialize_SingleInteger_AsInteger()
        {
            var json = @"{ ""property3"": 1 }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json, _settings);

            obj.Should().NotBeNull();
            obj!.Property3List.Should().HaveCount(1).And.Contain(1);
        }

        [Fact]
        public void Deserialize_SingleInteger_AsString()
        {
            var json = @"{ ""property3"": ""1"" }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json, _settings);

            obj.Should().NotBeNull();
            obj!.Property3List.Should().HaveCount(1).And.Contain(1);
        }

        [Fact]
        public void Deserialize_MultipleOccurrences()
        {
            var json = @"{
            ""property3"": ""1"",
            ""property3"": 2,
            ""property3"": ""3""
        }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json, _settings);

            obj.Should().NotBeNull();
            obj!.Property3List.Should().HaveCount(3).And.ContainInOrder(1, 2, 3);
        }

        [Fact]
        public void Deserialize_InvalidString_ThrowsException()
        {
            var json = @"{ ""property3"": ""invalid"" }";

            Action action = () => JsonConvert.DeserializeObject<TestClass>(json, _settings);

            action.Should().Throw<JsonException>().WithMessage("Failed to convert 'invalid' to integer.");
        }
    }

    public class SingleToListConverterTests
    {
        public class TestTSharkLayers
        {
            [JsonProperty("tls")]
            [JsonConverter(typeof(TestTlsToListConverter))]
            public List<TestTSharkTls>? Tls { get; set; }

            public class TestTlsToListConverter : SingleToListConverter<TestTSharkTls>
            {
                public override TestTSharkTls HandleStringToken(string? str) => new() { Tls = str };
            }

            public class TestTSharkTls
            {
                public string? Tls { get; set; } // sometimes the entire class is just a string

                [JsonProperty("tls.alert_message")]
                public string? AlertMessage { get; set; }

                [JsonProperty("tls.session_id")]
                public string? SessionId { get; set; }
            }
        }


        [Fact]
        public void Deserialize_WhenTlsIsString_HandlesCorrectly()
        {
            var json = @"{ ""tls"": ""someStringValue"" }";

            var obj = JsonConvert.DeserializeObject<TestTSharkLayers>(json);

            obj.Should().NotBeNull();
            obj!.Tls.Should().HaveCount(1);
            obj.Tls!.First().Tls.Should().Be("someStringValue");
        }

        [Fact]
        public void Deserialize_WhenTlsIsSingleObject_HandlesCorrectly()
        {
            var json = @"{
                ""tls"": {
                    ""tls.alert_message"": ""SomeAlert"",
                    ""tls.session_id"": ""SomeSessionId""
                }
            }";

            var obj = JsonConvert.DeserializeObject<TestTSharkLayers>(json);

            obj.Should().NotBeNull();
            obj!.Tls.Should().HaveCount(1);
            obj.Tls!.First().AlertMessage.Should().Be("SomeAlert");
            obj.Tls!.First().SessionId.Should().Be("SomeSessionId");
        }

        [Fact]
        public void Deserialize_WhenTlsIsArrayOfObjects_HandlesCorrectly()
        {
            var json = @"{
                ""tls"": [
                    {
                        ""tls.alert_message"": ""Alert1"",
                        ""tls.session_id"": ""Session1""
                    },
                    {
                        ""tls.alert_message"": ""Alert2"",
                        ""tls.session_id"": ""Session2""
                    }
                ]
            }";

            var obj = JsonConvert.DeserializeObject<TestTSharkLayers>(json);

            obj.Should().NotBeNull();
            obj!.Tls.Should().HaveCount(2);
            obj.Tls![0].AlertMessage.Should().Be("Alert1");
            obj.Tls[0].SessionId.Should().Be("Session1");
            obj.Tls[1].AlertMessage.Should().Be("Alert2");
            obj.Tls[1].SessionId.Should().Be("Session2");
        }

        [Fact]
        public void Deserialize_WhenMultipleTlsProperties_HandlesCorrectly()
        {
            var json = @"{
                ""tls"": {
                    ""tls.alert_message"": ""Alert1"",
                    ""tls.session_id"": ""Session1""
                },
                ""tls"": ""AnotherTlsString"",
                ""tls"": {
                    ""tls.alert_message"": ""Alert2"",
                    ""tls.session_id"": ""Session2""
                }
            }";

            var obj = JsonConvert.DeserializeObject<TestTSharkLayers>(json);

            obj.Should().NotBeNull();
            obj!.Tls.Should().HaveCount(3);

            obj.Tls![0].AlertMessage.Should().Be("Alert1");
            obj.Tls[0].SessionId.Should().Be("Session1");

            obj.Tls[1].Tls.Should().Be("AnotherTlsString");
            obj.Tls[1].AlertMessage.Should().BeNull();
            obj.Tls[1].SessionId.Should().BeNull();

            obj.Tls[2].AlertMessage.Should().Be("Alert2");
            obj.Tls[2].SessionId.Should().Be("Session2");
        }
    }

    public class KeyValuePairConverterTests
    {
        [Fact]
        public void ShouldConvertCustomJsonToDictionary()
        {
            // Arrange
            var json = @"{
                ""key"": ""key1"",
                ""value"": [""red"", ""orange""],
                ""key"": ""key2"",
                ""value"": [""green"", ""grape""]
            }";
            var expectedDict = new Dictionary<string, List<string>>
            {
                { "key1", new List<string> { "red", "orange" } },
                { "key2", new List<string> { "green", "grape" } }
            };

            // Act
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new TestConverter());
            var actualDict = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json, settings);

            // Assert
            actualDict.Should().BeEquivalentTo(expectedDict);
        }

        private class TestConverter : KeyedPairToDictConverter<List<string>>
        {
            public override string KeyPropertyName => "key";
            public override string ValuePropertyName => "value";
        }
    }

    public class ErrorOnDupesConverterTests
    {
        [JsonConverter(typeof(TestConverter))]
        public class TestClass
        {
            [JsonProperty("property1")]
            public string? Property1 { get; set; }

            [JsonProperty("property2")]
            public int Property2 { get; set; }

            [JsonProperty("property3")]
            [JsonConverter(typeof(IntToListConverter))]
            public List<int>? Property3List { get; set; }

            [JsonProperty("nested.property")]
            public NestClass? NestedProperty { get; set; }

            public class NestClass
            {
                [JsonProperty("sub.property")]
                public string? Property { get; set; }
            }

            public class TestConverter : ErrorOnDupesConverter<TestClass> { }
        }

        [Fact]
        public void DeserializeWithDuplicateKeys_ThrowsJsonException()
        {
            var json = @"{
                ""property1"": ""value1"",
                ""property1"": ""value2"",
                ""property2"": 42
            }";

            var exception = Assert.Throws<JsonSerializationException>(() =>
                JsonConvert.DeserializeObject<TestClass>(json));

            exception.Message.Should().Contain("Duplicate property 'property1' found");
        }

        [Fact]
        public void DeserializeWithoutDuplicateKeys_DeserializesCorrectly()
        {
            var json = @"{
                ""property1"": ""value1"",
                ""property2"": 42
            }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json);

            obj.Should().NotBeNull();
            obj!.Property1.Should().Be("value1");
            obj.Property2.Should().Be(42);
        }


        [Fact]
        public void DeserializeWithIgnoredDuplicateKeys_HandlesCorrectly()
        {
            var json = @"{
                ""ignoredProperty"": ""value1"",
                ""ignoredProperty"": ""value2"",
                ""property2"": 42
            }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json);

            obj.Should().NotBeNull();
            obj!.Property2.Should().Be(42);
        }


        [Fact]
        public void DeserializeWithoutDuplicateKeys_DeserializesCorrectly2()
        {
            var json = @"{
                ""property1"": ""value1"",
                ""property2"": 42,
                ""property3"": 1
            }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json);

            obj.Should().NotBeNull();
            obj!.Property1.Should().Be("value1");
            obj.Property2.Should().Be(42);
            obj.Property3List.Should().ContainSingle().Which.Should().Be(1);
        }

        [Fact]
        public void DeserializeWithDuplicateKeys_ThrowsJsonException2()
        {
            string _jsonWithDuplicates = @"{
                ""property1"": ""value1"",
                ""property1"": ""value2"",
                ""property2"": 42,
                ""property3"": ""1"",
                ""property3"": ""2"",
                ""property3"": ""3""
            }";

            var exception = Assert.Throws<JsonSerializationException>(() =>
                JsonConvert.DeserializeObject<TestClass>(_jsonWithDuplicates));

            exception.Message.Should().Contain("Duplicate property 'property1' found");
        }

        [Fact]
        public void DeserializeWithDuplicateKeysInConverter_WhenConverterIgnored_HandlesCorrectly()
        {
            string _jsonWithDuplicates = @"{
                ""property1"": ""value1"",
                ""property2"": 42,
                ""property3"": ""1"",
                ""property3"": ""2"",
                ""property3"": ""3""
            }";

            var obj = JsonConvert.DeserializeObject<TestClass>(_jsonWithDuplicates);

            obj.Should().NotBeNull();
            obj!.Property1.Should().Be("value1");
            obj.Property2.Should().Be(42);
            obj.Property3List.Should().HaveCount(3).And.ContainInOrder(1, 2, 3);
        }

        [Fact]
        public void DeserializeEmptyJsonObject_ReturnsDefaultValues()
        {
            var json = "{}";

            var obj = JsonConvert.DeserializeObject<TestClass>(json);

            obj.Should().NotBeNull();
            obj!.Property1.Should().BeNull();
            obj.Property2.Should().Be(0);
        }

        [Fact]
        public void DeserializeWithNestedJsonObject_HandlesCorrectly()
        {
            var json = @"{
                ""property1"": ""value1"",
                ""property2"": 42,
                ""nested.property"": { ""sub.property"": ""subValue1"" }
            }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json);

            obj.Should().NotBeNull();
            obj!.Property1.Should().Be("value1");
            obj.Property2.Should().Be(42);
            obj.NestedProperty.Should().NotBeNull();
            obj.NestedProperty!.Property.Should().Be("subValue1");
        }


        [Fact]
        public void DeserializeWithReversedPropertyOrder_HandlesCorrectly()
        {
            var json = @"{
                ""nested.property2"": { ""sub.property"": ""subValue1"" },
                ""property1"": ""value1""
            }";

            var obj = JsonConvert.DeserializeObject<TestClass>(json);

            obj.Should().NotBeNull();
            obj!.Property1.Should().Be("value1");
        }

    }

}
