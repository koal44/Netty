using FluentAssertions;
using Netryoshka;
using Newtonsoft.Json;

namespace Tests
{
    public class WireSharkPacketTests
    {
        public class TSharkJsonTests
        {
            private readonly string _json = /*lang=json,strict*/ @"{
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
            var obj = JsonConvert.DeserializeObject<TSharkIp>(_json);

            obj.Should().NotBeNull();
            obj!.Version.Should().Be("4");
            obj.Src.Should().Be("192.168.0.110");
            obj.Dst.Should().Be("1.1.1.1");
            obj.Ttl.Should().Be("64");
            obj.Proto.Should().Be("6");
            obj.Flags.Should().Be("0x00");
        }
    }


    public class TSharkTcpTests
    {
        private readonly string _json = @"{
            ""tcp.srcport"": ""12345"",
            ""tcp.dstport"": ""443"",
            ""tcp.port"": ""443"",
            ""tcp.stream"": ""0"",
            ""tcp.len"": ""0"",
            ""tcp.seq"": ""0"",
            ""tcp.nxtseq"": ""0"",
            ""tcp.ack"": ""0"",
            ""tcp.hdr_len"": ""20"",
            ""tcp.flags"": ""0x00000002"",
            ""tcp.flags_tree"": {
              ""tcp.flags.res"": ""0"",
              ""tcp.flags.ns"": ""0"",
              ""tcp.flags.cwr"": ""0"",
              ""tcp.flags.ecn"": ""0"",
              ""tcp.flags.urg"": ""0"",
              ""tcp.flags.ack"": ""1"",
              ""tcp.flags.push"": ""0"",
              ""tcp.flags.reset"": ""0"",
              ""tcp.flags.syn"": ""0"",
              ""tcp.flags.fin"": ""0""
            },
            ""tcp.window_size"": ""65535"",
            ""tcp.checksum"": ""0x0000"",
            ""tcp.checksum.status"": ""2"",
            ""tcp.urgent_pointer"": ""0"",
            ""tcp.options"": ""(none)"",
            ""tcp.analysis"": {
              ""tcp.analysis.bytes_in_flight"": ""0"",
              ""tcp.analysis.push_bytes_sent"": ""0"",
              ""tcp.analysis.retransmission"": ""0"",
              ""tcp.analysis.duplicate_ack"": ""0"",
              ""tcp.analysis.out_of_order"": ""0"",
              ""tcp.analysis.window_full"": ""0"",
              ""tcp.analysis.window_update"": ""0"",
              ""tcp.analysis.ack_lost_segment"": ""0"",
              ""tcp.analysis.fast_retransmission"": ""0"",
              ""tcp.analysis.spurious_retransmission"": ""0"",
              ""tcp.analysis.bytes_retransmitted"": ""0"",
              ""tcp.analysis.dup_ack"": ""0"",
              ""tcp.analysis.reused_ports"": ""0"",
              ""tcp.analysis.keep_alive"": ""0"",
              ""tcp.analysis.out_of_order_bytes"": ""0"",
              ""tcp.analysis.paws_update"": ""0"",
              ""tcp.analysis.window_shrink"": ""0"",
              ""tcp.analysis.window_expand"": ""0"",
              ""tcp.analysis.missed_segment"": ""0"",
              ""tcp.analysis.fast_retransmission_ack"": ""0"",
              """;
    }


}
