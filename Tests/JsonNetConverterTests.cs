using FluentAssertions;
using Netryoshka.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Xunit;

namespace Tests
{
    public class IntToListConverterTests
    {
        public class TestClass
        {
            [JsonProperty("property3")]
            [JsonConverter(typeof(IntToListConverter))]
            public List<int>? Property3List { get; set; }
        }

        private readonly JsonSerializerSettings _settings = new()
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
                //public override TestTSharkTls FallbackDeserializeFromString(string? str) => new() { FallbackString = str };
            }


            public class TestTSharkTls : IFallbackString
            {
                public string? FallbackString { get; set; }

                [JsonProperty("tls.alert_message")]
                public string? AlertMessage { get; set; }

                [JsonProperty("tls.session_id")]
                public string? SessionId { get; set; }

                [JsonProperty("tls.record")]
                [JsonConverter(typeof(TestTlsRecordToListConverter))]
                public List<TestTlsRecord>? Records { get; set; }

                public class TestTlsRecord : IFallbackString
                {
                    public string? Name { get; set; }
                    public string? FallbackString { get; set; }
                }

                public class TestTlsRecordToListConverter : SingleToListConverter<TestTlsRecord> { }
            }
        }


        [Fact]
        public void Deserialize_WhenTlsIsString_HandlesCorrectly()
        {
            var json = @"{ ""tls"": ""someStringValue"" }";

            var obj = JsonConvert.DeserializeObject<TestTSharkLayers>(json);

            obj.Should().NotBeNull();
            obj!.Tls.Should().HaveCount(1);
            obj.Tls!.First().FallbackString.Should().Be("someStringValue");
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

            var expectedObj = new TestTSharkLayers
            {
                Tls = new List<TestTSharkLayers.TestTSharkTls>
                {
                    new TestTSharkLayers.TestTSharkTls
                    {
                        AlertMessage = "Alert1",
                        SessionId = "Session1"
                    },
                    new TestTSharkLayers.TestTSharkTls
                    {
                        AlertMessage = "Alert2",
                        SessionId = "Session2"
                    }
                }
            };

            var actualObj = JsonConvert.DeserializeObject<TestTSharkLayers>(json);
            actualObj.Should().BeEquivalentTo(expectedObj);
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

            obj.Tls[1].FallbackString.Should().Be("AnotherTlsString");
            obj.Tls[1].AlertMessage.Should().BeNull();
            obj.Tls[1].SessionId.Should().BeNull();

            obj.Tls[2].AlertMessage.Should().Be("Alert2");
            obj.Tls[2].SessionId.Should().Be("Session2");
        }


        [Fact]
        public void Should_Deserialize_MultipleTlsObjects_WithNestedProperties()
        {
            var json = @"{
                ""tls"": {
                    ""tls.alert_message"": ""Alert1"",
                    ""tls.session_id"": ""SessionId1"",
                    ""tls.record"": {
                        ""Name"": ""Record11""
                    }
                },
                ""tls"": {
                    ""tls.alert_message"": ""Alert2"",
                    ""tls.session_id"": ""SessionId2"",
                    ""tls.record"": {
                        ""Name"": ""Record21""
                    },
                    ""tls.record"": {
                        ""Name"": ""Record22""
                    }
                }
            }";

            var ActualObject = JsonConvert.DeserializeObject<TestTSharkLayers>(json);

            var ExpectedObject = new TestTSharkLayers
            {
                Tls = new List<TestTSharkLayers.TestTSharkTls>
                {
                    new TestTSharkLayers.TestTSharkTls
                    {
                        AlertMessage = "Alert1",
                        SessionId = "SessionId1",
                        Records = new List<TestTSharkLayers.TestTSharkTls.TestTlsRecord>
                        {
                            new TestTSharkLayers.TestTSharkTls.TestTlsRecord { Name = "Record11" }
                        }
                    },
                    new TestTSharkLayers.TestTSharkTls
                    {
                        AlertMessage = "Alert2",
                        SessionId = "SessionId2",
                        Records = new List<TestTSharkLayers.TestTSharkTls.TestTlsRecord>
                        {
                            new TestTSharkLayers.TestTSharkTls.TestTlsRecord { Name = "Record21" },
                            new TestTSharkLayers.TestTSharkTls.TestTlsRecord { Name = "Record22" }
                        }
                    }
                }
            };

            ActualObject.Should().NotBeNull();
            ActualObject!.Should().BeEquivalentTo(ExpectedObject);
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
            public NestedClass? NestedProperty { get; set; }

            [JsonProperty(@"REGEX_.*alpha.*")]
            public string? AlphaProperty { get; set; }

            [JsonProperty(@"REGEX_.*beta.*")]
            public NestedClass? BetaProperty { get; set; }

            [JsonProperty("nested.property4")]
            [JsonConverter(typeof(NestedClassToListConverter))]
            public List<NestedClass>? NestedList { get; set; }


            public class NestedClass : IFallbackString
            {
                public string? FallbackString { get; set; }

                [JsonProperty("sub.property")]
                public string? Property { get; set; }
            }


            public class TestConverter : ErrorOnDupesConverter<TestClass>
            {
                protected override void HandleDynamicProperty(string propertyName, JsonProperty property, TestClass instance)
                {
                    if (propertyName.Contains("alpha"))
                    {
                        instance.Property1 = $"***{propertyName}***";
                        property.Ignored = true; // this will skip over the dynamic property valueobject
                    }
                    else if (propertyName.Contains("beta"))
                    {
                        instance.AlphaProperty = propertyName;
                    }
                }
            }


            public class NestedClassToListConverter : SingleToListConverter<NestedClass> { }
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


        [Fact]
        public void DeserializeWithAlphaDynamicKey_HandlesCorrectly()
        {
            var json = @"{
                ""property1"": ""value1"",
                ""property2"": 42,
                ""dynamic_alpha_key"": ""someValue""
            }";

            var expected = new TestClass
            {
                Property1 = "***dynamic_alpha_key***",
                Property2 = 42,
                AlphaProperty = null
            };

            var actual = JsonConvert.DeserializeObject<TestClass>(json);

            actual.Should().BeEquivalentTo(expected);
        }


        // NOTE: This test would fail if property1 came after dynamic_beta_key in the JSON. The test is just meant to showcase what HandleDynamicProperty() can do.
        [Fact]
        public void DeserializeWithBetaDynamicKey_HandlesCorrectly()
        {
            var json = @"{
                ""property1"": ""value1"",
                ""property2"": 42,
                ""dynamic_beta_key"": { ""sub.property"": ""nestedValue"" }
            }";

            var expected = new TestClass
            {
                Property1 = "value1",
                Property2 = 42,
                AlphaProperty = "dynamic_beta_key",
                BetaProperty = new TestClass.NestedClass { Property = "nestedValue" }
            };

            var actual = JsonConvert.DeserializeObject<TestClass>(json);

            actual.Should().BeEquivalentTo(expected);
        }


        [Fact]
        public void ShouldDeserializeDuplicateNestedProperty4IntoList()
        {
            // Arrange
            string json = @"{
                ""nested.property4"": {
                    ""sub.property"": ""value1""
                },
                ""nested.property4"": {
                    ""sub.property"": ""value2""
                }
            }";

            List<TestClass.NestedClass> expectedNestedList = new()
            {
                new TestClass.NestedClass { Property = "value1" },
                new TestClass.NestedClass { Property = "value2" }
            };

            // Act
            var actualObject = JsonConvert.DeserializeObject<TestClass>(json);

            // Assert
            actualObject.Should().NotBeNull();
            actualObject!.NestedList.Should().BeEquivalentTo(expectedNestedList);
        }


    }

}
