using FluentAssertions;
using Netryoshka.Utils;
using Xunit;

namespace Tests
{
    public class JsonUtilsTests
    {
        [Fact]
        public void SplitJsonObjects_EmptyArray_ReturnsEmptyList()
        {
            var json = @"[]";
            var result = JsonUtils.SplitJsonObjects(json);
            result.Should().BeEmpty();
        }

        [Fact]
        public void SplitJsonObjects_SingleFieldObjects_ReturnsListOfObjects()
        {
            var json = @"[{""a"": 1}, {""b"": 2}]";
            var result = JsonUtils.SplitJsonObjects(json);
            result.Should().BeEquivalentTo(new List<string> { @"{""a"": 1}", @"{""b"": 2}" });
        }

        [Fact]
        public void SplitJsonObjects_MultiFieldObjects_ReturnsListOfObjects()
        {
            var json = @"[{""a"": 1, ""b"": 2}]";
            var result = JsonUtils.SplitJsonObjects(json);
            result.Should().BeEquivalentTo(new List<string> { @"{""a"": 1, ""b"": 2}" });
        }

        [Fact]
        public void SplitJsonObjects_NestedObjects_ReturnsListOfObjects()
        {
            var json = @"[{""a"": {""b"": 1}}]";
            var result = JsonUtils.SplitJsonObjects(json);
            result.Should().BeEquivalentTo(new List<string> { @"{""a"": {""b"": 1}}" });
        }

        [Fact]
        public void SplitJsonObjects_MixedObjectsAndArrays_ReturnsListOfObjects()
        {
            var json = @"[{""a"": 1}, [1, 2, 3], {""b"": 2}]";
            var result = JsonUtils.SplitJsonObjects(json);
            result.Should().BeEquivalentTo(new List<string> { @"{""a"": 1}", @"{""b"": 2}" }); // Assumes your method only looks for objects
        }

        [Fact]
        public void ExtractJsonObjectsFromKey_FindsHttp2Objects()
        {
            // Arrange
            string json = @"
            {
                ""_source"": {
                    ""layers"": {
                        ""frame"": {
                            ""frame.section_number"": 1
                        },
                        ""eth"": {
                            ""eth.dst"": ""00:1A:2B:3C:4D:5E""
                        },
                        ""ip"": {
                            ""ip.version"": ""IPv4""
                        },
                        ""tcp"": {
                            ""tcp.srcport"": ""8080""
                        },
                        ""tcp.segments"": {
                            ""tcp.segment"": ""1"",
                            ""tcp.segment"": ""2""
                        },
                        ""http"": {
                            ""http.response.line"": ""Content-Type: text/html""
                        },
                        ""http2"": {""http2.stream"": {""http2.magic"": ""magic_string""}},
                        ""http2"": {""http2.stream"": {""http2.magic"": ""magic_string2""}}
                    }
                }
            }";

            // Act
            var extractedObjects = JsonUtils.ExtractJsonObjectsFromKey(json, "http2");

            // Assert
            extractedObjects.Count.Should().Be(2);
            extractedObjects[0].Should().Be(@"{""http2.stream"": {""http2.magic"": ""magic_string""}}");
            extractedObjects[1].Should().Be(@"{""http2.stream"": {""http2.magic"": ""magic_string2""}}");
        }

    }
}
