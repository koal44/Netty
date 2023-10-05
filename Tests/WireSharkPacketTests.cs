using FluentAssertions;
using Netryoshka;
using Newtonsoft.Json;

namespace Tests
{
    public class WireSharkPacketTests
    {
        public class TSharkJsonTests
        {
            private string _json = /*lang=json,strict*/ @"{
                ""json.object"": {
                ""json.member"": ""detail"",
                ""json.member_tree"": {
                    ""json.object"": {
                    ""json.member"": ""message"",
                    ""json.member_tree"": {
                        ""json.path_with_value"": ""/detail/message:Your authentication token has expired."",
                        ""json.member_with_value"": ""message:Your authentication token has expired."",
                        ""json.value.string"": ""Your authentication token has expired."",
                        ""json.key"": ""message"",
                        ""json.path"": ""/detail/message""
                    },
                    ""json.member"": ""type"",
                    ""json.member_tree"": {
                        ""json.path_with_value"": ""/detail/type:invalid_request_error"",
                        ""json.member_with_value"": ""type:invalid_request_error"",
                        ""json.value.string"": ""invalid_request_error"",
                        ""json.key"": ""type"",
                        ""json.path"": ""/detail/type""
                    },
                    ""json.member"": ""param"",
                    ""json.member_tree"": {
                        ""json.path_with_value"": ""/detail/param:null"",
                        ""json.member_with_value"": ""param:null"",
                        ""json.value.null"": """",
                        ""json.key"": ""param"",
                        ""json.path"": ""/detail/param""
                    },
                    ""json.member"": ""code"",
                    ""json.member_tree"": {
                        ""json.path_with_value"": ""/detail/code:token_expired"",
                        ""json.member_with_value"": ""code:token_expired"",
                        ""json.value.string"": ""token_expired"",
                        ""json.key"": ""code"",
                        ""json.path"": ""/detail/code""
                    }
                    },
                    ""json.key"": ""detail"",
                    ""json.path"": ""/detail""
                }
                }
            }";
            [Fact]
            public void ShouldDeserializeJsonObjectIntoDictionary()
            {
                // Act
                var result = JsonConvert.DeserializeObject<TSharkJson>(_json);

                // Assert
                result.Should().NotBeNull();
                result!.Object.Should().NotBeNull();
                result!.Object.Should().ContainKey("detail");

                var detailMemberTree = result!.Object!["detail"];
                detailMemberTree.Should().NotBeNull();
                detailMemberTree!.Object.Should().NotBeNull();
                detailMemberTree!.Object.Should().ContainKey("message");

                var messageMember = detailMemberTree!.Object!["message"];
                messageMember.Should().NotBeNull();
                messageMember!.ValueString.Should().Be("Your authentication token has expired.");
                messageMember!.Key.Should().Be("message");
                messageMember!.Path.Should().Be("/detail/message");
            }
        }
    }


    public class TSharkIpTests
    {
        private readonly string _json = @"{
            ""ip.version"": ""4"",
            ""ip.hdr_len"": ""20"",
            ""ip.dsfield"": ""0x00"",
            ""ip.dsfield_tree"": {
              ""ip.dsfield.dscp"": ""0"",
              ""ip.dsfield.ecn"": ""0""
            },
            ""ip.len"": ""40"",
            ""ip.id"": ""0x0000"",
            ""ip.flags"": ""0x00"",
            ""ip.flags_tree"": {
              ""ip.flags.rb"": ""0"",
              ""ip.flags.df"": ""0"",
              ""ip.flags.mf"": ""0""
            },
            ""ip.frag_offset"": ""0"",
            ""ip.ttl"": ""64"",
            ""ip.proto"": ""6"",
            ""ip.checksum"": ""0x0000"",
            ""ip.checksum.status"": ""2"",
            ""ip.src"": ""192.168.0.110"",
            ""ip.addr"": ""192.168.0.110"",
            ""ip.src_host"": ""192.168.0.110"",
            ""ip.host"": ""192.168.0.110"",
            ""ip.dst"": ""1.1.1.1"",
            ""ip.addr"": ""1.1.1.1"",
            ""ip.dst_host"": ""1.1.1.1"",
            ""ip.host"": ""1.1.1.1""
        }";

        [Fact]
        public void ShouldUseCustomConverter()
        {
            var deserializedObject = JsonConvert.DeserializeObject<TSharkIp>(_json);

            // This test will fail but you should be able to hit the breakpoint in DummyConverter
            deserializedObject.Should().NotBeNull();
            deserializedObject.Version.Should().Be("5"); // Deliberate failure
        }
    }
}
