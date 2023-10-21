using FluentAssertions;
using Netryoshka;
using Newtonsoft.Json;

namespace Tests
{

    // TODO: Refactor tests to use JSON fixtures instead of hardcoded strings for better maintainability.

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
            public void ShouldDeserializeJsonObjectIntoDictionary1()
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

            [Fact]
            public void ShouldDeserializeJsonObjectIntoDictionary2()
            {
                // Act
                var actualResult = JsonConvert.DeserializeObject<TSharkJson>(_json);

                // Assert
                var expectedResult = new TSharkJson
                {
                    Object = new Dictionary<string, TSharkJson.JsonMemberTree>
                    {
                        ["detail"] = new TSharkJson.JsonMemberTree
                        {
                            Object = new Dictionary<string, TSharkJson.JsonMemberTree>
                            {
                                ["message"] = new TSharkJson.JsonMemberTree
                                {
                                    ValueString = "Your authentication token has expired.",
                                    Key = "message",
                                    Path = "/detail/message"
                                },
                                ["type"] = new TSharkJson.JsonMemberTree
                                {
                                    ValueString = "invalid_request_error",
                                    Key = "type",
                                    Path = "/detail/type"
                                },
                                ["param"] = new TSharkJson.JsonMemberTree
                                {
                                    ValueNull = "",
                                    Key = "param",
                                    Path = "/detail/param"
                                },
                                ["code"] = new TSharkJson.JsonMemberTree
                                {
                                    ValueString = "token_expired",
                                    Key = "code",
                                    Path = "/detail/code"
                                }
                            },
                            Key = "detail",
                            Path = "/detail"
                        }
                    }
                };

                actualResult.Should().BeEquivalentTo(expectedResult);
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
              }";

        [Fact]
        public void DeserializeJson_ShouldPopulateTSharkTcpObject()
        {
            var expectedTSharkTcp = new TSharkTcp
            {
                SrcPort = "12345",
                DstPort = "443",
                Stream = "0",
                Len = "0",
                Seq = "0",
                HdrLen = "20",
                WindowSize = "65535",
                Checksum = "0x0000",
                ChecksumStatus = "Unverified",
                Flags = new TSharkTcp.TSharkTcpFlags
                {
                    Ack = true,
                    Res = false,
                    Ae = false,
                    Cwr = false,
                    Ece = false,
                    Urg = false,
                    Push = false,
                    Reset = false,
                    Syn = false,
                    Fin = false
                },
            };

            // Act
            var actualTSharkTcp = JsonConvert.DeserializeObject<TSharkTcp>(_json);

            // Assert
            actualTSharkTcp.Should().BeEquivalentTo(expectedTSharkTcp);
        }
    }


    public class TSharkEthTests
    {
        private readonly string _json = @"{
          ""eth.dst"": ""a0:29:42:37:ef:4c"",
          ""eth.dst_tree"": {
            ""eth.dst_resolved"": ""Intel_37:ef:4c"",
            ""eth.dst.oui"": ""10496322"",
            ""eth.dst.oui_resolved"": ""Intel Corporate"",
            ""eth.addr"": ""a0:29:42:37:ef:4c"",
            ""eth.addr_resolved"": ""Intel_37:ef:4c"",
            ""eth.addr.oui"": ""10496322"",
            ""eth.addr.oui_resolved"": ""Intel Corporate"",
            ""eth.dst.lg"": ""0"",
            ""eth.lg"": ""0"",
            ""eth.dst.ig"": ""0"",
            ""eth.ig"": ""0""
          },
          ""eth.src"": ""30:b5:c2:e4:e0:b2"",
          ""eth.src_tree"": {
            ""eth.src_resolved"": ""TpLinkTechno_e4:e0:b2"",
            ""eth.src.oui"": ""3192258"",
            ""eth.src.oui_resolved"": ""Tp-Link Technologies Co.,Ltd."",
            ""eth.addr"": ""30:b5:c2:e4:e0:b2"",
            ""eth.addr_resolved"": ""TpLinkTechno_e4:e0:b2"",
            ""eth.addr.oui"": ""3192258"",
            ""eth.addr.oui_resolved"": ""Tp-Link Technologies Co.,Ltd."",
            ""eth.src.lg"": ""0"",
            ""eth.lg"": ""0"",
            ""eth.src.ig"": ""0"",
            ""eth.ig"": ""0""
          },
          ""eth.type"": ""0x0800""
        }";

        [Fact]
        public void ShouldDeserializeCorrectly()
        {
            // Arrange
            var expectedObject = new TSharkEth
            {
                Dst = "a0:29:42:37:ef:4c",
                Src = "30:b5:c2:e4:e0:b2",
                EthType = "0x0800",
                DstTree = new TSharkEth.EthernetDstTree
                {
                    DstOuiResolved = "Intel Corporate"
                },
                SrcTree = new TSharkEth.EthernetSrcTree
                {
                    SrcOuiResolved = "Tp-Link Technologies Co.,Ltd."
                },
                EthTypeVal = "IPv4"
            };

            // Act
            var actualObject = JsonConvert.DeserializeObject<TSharkEth>(_json);

            // Assert
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
    }


    public class TSharkHttpTests
    {
        private readonly string _jsonRequest = @"{
          ""GET / HTTP/1.1\\r\\n"": {
            ""_ws.expert"": {
              ""http.chat"": """",
              ""_ws.expert.message"": ""GET / HTTP/1.1\\r\\n"",
              ""_ws.expert.severity"": ""2097152"",
              ""_ws.expert.group"": ""33554432""
            },
            ""http.request.method"": ""GET"",
            ""http.request.uri"": ""/"",
            ""http.request.version"": ""HTTP/1.1""
          },
          ""http.host"": ""portquiz.net"",
          ""http.request.line"": ""Host: portquiz.net\r\n"",
          ""http.user_agent"": ""Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0"",
          ""http.request.line"": ""User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0\r\n"",
          ""http.accept"": ""text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8"",
          ""http.request.line"": ""Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\n"",
          ""http.accept_language"": ""en-US,en;q=0.5"",
          ""http.request.line"": ""Accept-Language: en-US,en;q=0.5\r\n"",
          ""http.accept_encoding"": ""gzip, deflate"",
          ""http.request.line"": ""Accept-Encoding: gzip, deflate\r\n"",
          ""http.referer"": ""https://www.google.com/"",
          ""http.request.line"": ""Referer: https://www.google.com/\r\n"",
          ""http.request.line"": ""DNT: 1\r\n"",
          ""http.connection"": ""keep-alive"",
          ""http.request.line"": ""Connection: keep-alive\r\n"",
          ""http.request.line"": ""Upgrade-Insecure-Requests: 1\r\n"",
          ""\\r\\n"": """",
          ""http.request.full_uri"": ""http://portquiz.net/"",
          ""http.request"": ""1"",
          ""http.request_number"": ""1""
        }";

        private readonly string _jsonResponse = @"{
          ""HTTP/1.1 200 OK\\r\\n"": {
            ""_ws.expert"": {
              ""http.chat"": """",
              ""_ws.expert.message"": ""HTTP/1.1 200 OK\\r\\n"",
              ""_ws.expert.severity"": ""2097152"",
              ""_ws.expert.group"": ""33554432""
            },
            ""http.response.version"": ""HTTP/1.1"",
            ""http.response.code"": ""200"",
            ""http.response.code.desc"": ""OK"",
            ""http.response.phrase"": ""OK""
          },
          ""http.date"": ""Mon, 16 Oct 2023 16:53:34 GMT"",
          ""http.response.line"": ""Date: Mon, 16 Oct 2023 16:53:34 GMT\r\n"",
          ""http.server"": ""Apache/2.4.29 (Ubuntu)"",
          ""http.response.line"": ""Server: Apache/2.4.29 (Ubuntu)\r\n"",
          ""http.response.line"": ""Vary: Accept-Encoding\r\n"",
          ""http.content_encoding"": ""gzip"",
          ""http.response.line"": ""Content-Encoding: gzip\r\n"",
          ""http.content_length_header"": ""1197"",
          ""http.content_length_header_tree"": {
            ""http.content_length"": ""1197""
          },
          ""http.response.line"": ""Content-Length: 1197\r\n"",
          ""http.connection"": ""close"",
          ""http.response.line"": ""Connection: close\r\n"",
          ""http.content_type"": ""text/html; charset=UTF-8"",
          ""http.response.line"": ""Content-Type: text/html; charset=UTF-8\r\n"",
          ""\\r\\n"": """",
          ""http.response"": ""1"",
          ""http.response_number"": ""1"",
          ""http.time"": ""0.169084000"",
          ""http.request_in"": ""4"",
          ""http.response_for.uri"": ""http://portquiz.net/"",
          ""Content-encoded entity body (gzip): 1197 bytes -> 2543 bytes"": """",
          ""http.file_data"": ""\n<html>\n<head>\n<title>Outgoing Port Tester<\/title>\nbody {\n\tfont-family: sans-serif;\n\tfont-size: 0.9em;\n}\n<\/style>\n\n<\/head>\n\n<body><\/body>\n\n<\/html>\n""
        }";

        [Fact]
        public void ShouldDeserializeHttpRequestCorrectly()
        {
            // Arrange
            var expectedObject = new TSharkHttp
            {
                Declaration = "GET / HTTP/1.1\\r\\n",
                RequestLines = new List<string>
                {
                    "Host: portquiz.net\r\n",
                    "User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0\r\n",
                    "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\n",
                    "Accept-Language: en-US,en;q=0.5\r\n",
                    "Accept-Encoding: gzip, deflate\r\n",
                    "Referer: https://www.google.com/\r\n",
                    "DNT: 1\r\n",
                    "Connection: keep-alive\r\n",
                    "Upgrade-Insecure-Requests: 1\r\n"
                },
                Lines = new Dictionary<string, string>
                {
                    ["Host"] = "portquiz.net\r\n",
                    ["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/118.0\r\n",
                    ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,*/*;q=0.8\r\n",
                    ["Accept-Language"] = "en-US,en;q=0.5\r\n",
                    ["Accept-Encoding"] = "gzip, deflate\r\n",
                    ["Referer"] = "https://www.google.com/\r\n",
                    ["Connection"] = "keep-alive\r\n",
                    ["DNT"] = "1\r\n",
                    ["Upgrade-Insecure-Requests"] = "1\r\n"
                },
                RequestNumber = 1,
                FullUri = "http://portquiz.net/",
                Request = true,
            };

            // Act
            var actualObject = JsonConvert.DeserializeObject<TSharkHttp>(_jsonRequest);

            // Assert
            actualObject.Should().BeEquivalentTo(expectedObject);
        }

        [Fact]
        public void ShouldDeserializeHttpResponseCorrectly()
        {
            // Arrange
            var expectedObject = new TSharkHttp
            {
                Declaration = "HTTP/1.1 200 OK\\r\\n",
                ResponseLines = new List<string>
                {
                    "Date: Mon, 16 Oct 2023 16:53:34 GMT\r\n",
                    "Server: Apache/2.4.29 (Ubuntu)\r\n",
                    "Vary: Accept-Encoding\r\n",
                    "Content-Encoding: gzip\r\n",
                    "Content-Length: 1197\r\n",
                    "Connection: close\r\n",
                    "Content-Type: text/html; charset=UTF-8\r\n"
                },
                Lines = new Dictionary<string, string>
                {
                    ["Date"] = "Mon, 16 Oct 2023 16:53:34 GMT\r\n",
                    ["Server"] = "Apache/2.4.29 (Ubuntu)\r\n",
                    ["Vary"] = "Accept-Encoding\r\n",
                    ["Content-Encoding"] = "gzip\r\n",
                    ["Content-Length"] = "1197\r\n",
                    ["Connection"] = "close\r\n",
                    ["Content-Type"] = "text/html; charset=UTF-8\r\n"
                },
                ContentLength = 1197,
                FileData = "\n<html>\n<head>\n<title>Outgoing Port Tester</title>\nbody {\n\tfont-family: sans-serif;\n\tfont-size: 0.9em;\n}\n</style>\n\n</head>\n\n<body></body>\n\n</html>\n",
                RequestIn = 4,
                ResponseNumber = 1,
                Time = 0.169084,
                ResponseForUri = "http://portquiz.net/",
                Response = true,
            };

            // Act
            var actualObject = JsonConvert.DeserializeObject<TSharkHttp>(_jsonResponse);

            // Assert
            actualObject.Should().BeEquivalentTo(expectedObject);
        }
    }


    public class TSharkTlsTests
    {
        private readonly string _json = @"{
            ""tls.record"": {
                ""tls.record.content_type"": ""22"",
                ""tls.record.version"": ""0x0301"",
                ""tls.record.length"": ""680"",
                ""tls.handshake"": {
                    ""tls.handshake.type"": ""1"",
                    ""tls.handshake.length"": ""676"",
                    ""tls.handshake.version"": ""0x0303"",
                    ""tls.handshake.random"": ""0c:d2:de:d2:3c:f8:dd:4a:79:75:b1:4f:c1:c1:ef:4d"",
                    ""tls.handshake.random_tree"": {
                        ""tls.handshake.random_time"": ""Oct 25, 1976 19:49:54.000000000 Pacific Daylight Time"",
                        ""tls.handshake.random_bytes"": ""3c:f8:dd:4a:79:75:2a:78:c1:86:4f:c1:c1:ef:4d""
                    },
                    ""tls.handshake.session_id_length"": ""32"",
                    ""tls.handshake.session_id"": ""66:38:16:e8:24:1c:f9:43:c3:4c:db:8c:db:9c:5d:4c:99"",
                    ""tls.handshake.cipher_suites_length"": ""34"",
                    ""tls.handshake.ciphersuites"": {
                        ""tls.handshake.ciphersuite"": ""0x0035""
                    },
                    ""tls.handshake.comp_methods_length"": ""1"",
                    ""tls.handshake.comp_methods"": {
                        ""tls.handshake.comp_method"": ""0""
                    },
                    ""tls.handshake.extensions_length"": ""569"",
                    ""Extension: server_name (len=42)"": {
                        ""tls.handshake.extension.type"": ""0"",
                        ""tls.handshake.extension.len"": ""42"",
                        ""Server Name Indication extension"": {
                            ""tls.handshake.extensions_server_name_list_len"": ""40"",
                            ""tls.handshake.extensions_server_name_type"": ""0"",
                            ""tls.handshake.extensions_server_name_len"": ""37"",
                            ""tls.handshake.extensions_server_name"": ""firefox.settings.services.mozilla.com""
                        }
                    },
                    ""Extension: extended_master_secret (len=0)"": {
                        ""tls.handshake.extension.type"": ""23"",
                        ""tls.handshake.extension.len"": ""0""
                    },
                    ""Extension: renegotiation_info (len=1)"": {
                        ""tls.handshake.extension.type"": ""65281"",
                        ""tls.handshake.extension.len"": ""1"",
                        ""Renegotiation Info extension"": {
                            ""tls.handshake.extensions_reneg_info_len"": ""0""
                        }
                    },
                    ""Extension: supported_groups (len=14)"": {
                        ""tls.handshake.extension.type"": ""10"",
                        ""tls.handshake.extension.len"": ""14"",
                        ""tls.handshake.extensions_supported_groups_length"": ""12"",
                        ""tls.handshake.extensions_supported_groups"": {
                            ""tls.handshake.extensions_supported_group"": ""0x0101""
                        }
                    },
                    ""Extension: ec_point_formats (len=2)"": {
                        ""tls.handshake.extension.type"": ""11"",
                        ""tls.handshake.extension.len"": ""2"",
                        ""tls.handshake.extensions_ec_point_formats_length"": ""1"",
                        ""tls.handshake.extensions_ec_point_formats"": {
                            ""tls.handshake.extensions_ec_point_format"": ""0""
                        }
                    },
                    ""Extension: session_ticket (len=0)"": {
                        ""tls.handshake.extension.type"": ""35"",
                        ""tls.handshake.extension.len"": ""0"",
                        ""tls.handshake.extensions.session_ticket"": """"
                    },
                    ""Extension: application_layer_protocol_negotiation (len=14)"": {
                        ""tls.handshake.extension.type"": ""16"",
                        ""tls.handshake.extension.len"": ""14"",
                        ""tls.handshake.extensions_alpn_len"": ""12"",
                        ""tls.handshake.extensions_alpn_list"": {
                            ""tls.handshake.extensions_alpn_str_len"": ""8"",
                            ""tls.handshake.extensions_alpn_str"": ""http/1.1""
                        }
                    },
                    ""Extension: status_request (len=5)"": {
                        ""tls.handshake.extension.type"": ""5"",
                        ""tls.handshake.extension.len"": ""5"",
                        ""tls.handshake.extensions_status_request_type"": ""1"",
                        ""tls.handshake.extensions_status_request_responder_ids_len"": ""0"",
                        ""tls.handshake.extensions_status_request_exts_len"": ""0""
                    },
                    ""Extension: delegated_credentials (len=10)"": {
                        ""tls.handshake.extension.type"": ""34"",
                        ""tls.handshake.extension.len"": ""10"",
                        ""tls.handshake.sig_hash_alg_len"": ""8"",
                        ""tls.handshake.sig_hash_algs"": {
                            ""tls.handshake.sig_hash_alg"": ""0x0203"",
                            ""tls.handshake.sig_hash_alg_tree"": {
                                ""tls.handshake.sig_hash_hash"": ""2"",
                                ""tls.handshake.sig_hash_sig"": ""3""
                            }
                        }
                    },
                    ""Extension: key_share (len=107)"": {
                        ""tls.handshake.extension.type"": ""51"",
                        ""tls.handshake.extension.len"": ""107"",
                        ""Key Share extension"": {
                            ""tls.handshake.extensions_key_share_client_length"": ""105"",
                            ""Key Share Entry: Group: x25519, Key Exchange length: 32"": {
                                ""tls.handshake.extensions_key_share_group"": ""29"",
                                ""tls.handshake.extensions_key_share_key_exchange_length"": ""32"",
                                ""tls.handshake.extensions_key_share_key_exchange"": ""1d:37:69:d2:a4:a9""
                            },
                            ""Key Share Entry: Group: secp256r1, Key Exchange length: 65"": {
                                ""tls.handshake.extensions_key_share_group"": ""23"",
                                ""tls.handshake.extensions_key_share_key_exchange_length"": ""65"",
                                ""tls.handshake.extensions_key_share_key_exchange"": ""04:3c:7c:00:70""
                            }
                        }
                    },
                    ""Extension: supported_versions (len=5)"": {
                        ""tls.handshake.extension.type"": ""43"",
                        ""tls.handshake.extension.len"": ""5"",
                        ""tls.handshake.extensions.supported_versions_len"": ""4"",
                        ""tls.handshake.extensions.supported_version"": ""0x0303""
                    },
                    ""Extension: signature_algorithms (len=24)"": {
                        ""tls.handshake.extension.type"": ""13"",
                        ""tls.handshake.extension.len"": ""24"",
                        ""tls.handshake.sig_hash_alg_len"": ""22"",
                        ""tls.handshake.sig_hash_algs"": {
                            ""tls.handshake.sig_hash_alg"": ""0x0201"",
                            ""tls.handshake.sig_hash_alg_tree"": {
                                ""tls.handshake.sig_hash_hash"": ""2"",
                                ""tls.handshake.sig_hash_sig"": ""1""
                            }
                        }
                    },
                    ""Extension: psk_key_exchange_modes (len=2)"": {
                        ""tls.handshake.extension.type"": ""45"",
                        ""tls.handshake.extension.len"": ""2"",
                        ""tls.extension.psk_ke_modes_length"": ""1"",
                        ""tls.extension.psk_ke_mode"": ""1""
                    },
                    ""Extension: record_size_limit (len=2)"": {
                        ""tls.handshake.extension.type"": ""28"",
                        ""tls.handshake.extension.len"": ""2"",
                        ""tls.record_size_limit"": ""16385""
                    },
                    ""Extension: Unknown type 65037 (len=281)"": {
                        ""tls.handshake.extension.type"": ""65037"",
                        ""tls.handshake.extension.len"": ""281"",
                        ""tls.handshake.extension.data"": ""00:00:01:00:03:91:00:20:ff:b3:4b""
                    },
                    ""tls.handshake.ja3_full"": ""771,4865-4867-28-65037,29-23-24-25-256-257,0"",
                    ""tls.handshake.ja3"": ""b5001237acdf006056b409cc433726b0""
                }
            }
        }";


        [Fact]
        public void ShouldBeSerializedTo_Test()
        {
            // Arrange
            var expectedObject = new TSharkTls
            {
                Tls = null,
                Records = new List<TSharkTls.TlsRecord>
                {
                    new TSharkTls.TlsRecord
                    {
                        ContentType = "22",
                        Version = "0x0301",
                        Length = 680,
                        Handshake = new List<TSharkTls.TlsHandshake>
                        {
                            new TSharkTls.TlsHandshake
                            {
                                HandshakeType = "1",
                                HandshakeVersion = TSharkTls.TlsHandshakeVersion.Tls12,
                                Random = "0c:d2:de:d2:3c:f8:dd:4a:79:75:b1:4f:c1:c1:ef:4d",
                                SessionId = "66:38:16:e8:24:1c:f9:43:c3:4c:db:8c:db:9c:5d:4c:99"
                            }
                        }
                    }
                },
                AlertMessage = null,
                SessionId = null
            };
            var actualobject = JsonConvert.DeserializeObject<TSharkTls>(_json);

            // Act & Assert
            expectedObject.Should().BeEquivalentTo(actualobject);
        }

        [Fact]
        public void ShouldBeSerializedTo_Test2()
        {
            // Arrange
            string _json = @"{
              ""tls.record"": {
                ""tls.record.opaque_type"": ""23"",
                ""tls.record.version"": ""0x0303"",
                ""tls.record.length"": ""185"",
                ""tls.record.content_type"": ""23"",
                ""tls.app_data"": ""8e:de:7b:2a:f3:68:f7:be:18:a4:32:dc:33:d3:1a:2e:89:13:3b:b1:34"",
                ""tls.app_data_proto"": ""HyperText Transfer Protocol 2""
              }
            }";

            var expectedObject = new TSharkTls
            {
                Tls = null,
                Records = new List<TSharkTls.TlsRecord>
                {
                    new TSharkTls.TlsRecord
                    {
                        ContentType = "23",
                        Version = "0x0303",
                        Length = 185,
                        OpaqueType = "23",
                        AppData = "8e:de:7b:2a:f3:68:f7:be:18:a4:32:dc:33:d3:1a:2e:89:13:3b:b1:34",
                        AppDataProtocol = "HyperText Transfer Protocol 2"
                    }
                },
                AlertMessage = null,
                SessionId = null
            };
            var actualObject = JsonConvert.DeserializeObject<TSharkTls>(_json);

            // Act & Assert
            expectedObject.Should().BeEquivalentTo(actualObject);
        }


        [Fact]
        public void ShouldBeSerializedTo_Test3()
        {
            // Arrange
            var _json = @"{
            ""tls"": {
              ""tls.record"": {
                ""tls.record.content_type"": ""22"",
                ""tls.record.version"": ""0x0303"",
                ""tls.record.length"": ""3042"",
                ""tls.handshake"": {
                  ""tls.handshake.type"": ""11"",
                  ""tls.handshake.length"": ""3038"",
                  ""tls.handshake.certificates_length"": ""3035"",
                  ""tls.handshake.certificates"": {
                    ""tls.handshake.certificate_length"": ""1767"",
                    ""tls.handshake.certificate"": ""2b:b2:95:13:c7:f9:e6:cd:2a:39:c8:0b"",
                    ""tls.handshake.certificate_tree"": {},
                    ""tls.handshake.certificate_length"": ""1262"",
                    ""tls.handshake.certificate"": ""13:d0:9c:c8:f2:4b:39:4f:52:84:49:a6:4c:90:4e:1f:f7:b4"",
                    ""tls.handshake.certificate_tree"": {}
                  }
                }
              }
            },
            ""tls"": {
              ""tls.record"": {
                ""tls.record.content_type"": ""22"",
                ""tls.record.version"": ""0x0303"",
                ""tls.record.length"": ""300"",
                ""tls.handshake"": {
                  ""tls.handshake.type"": ""12"",
                  ""tls.handshake.length"": ""296"",
                  ""EC Diffie-Hellman Server Params"": {
                    ""tls.handshake.server_curve_type"": ""0x03"",
                    ""tls.handshake.server_named_curve"": ""0x001d"",
                    ""tls.handshake.server_point_len"": ""32"",
                    ""tls.handshake.server_point"": ""b4:d4:85:50:3b:64:07:85:51:e0:23:d7:31:7e:be:47:1b:1e"",
                    ""tls.handshake.sig_hash_alg"": ""0x0804"",
                    ""tls.handshake.sig_hash_alg_tree"": {
                      ""tls.handshake.sig_hash_hash"": ""8"",
                      ""tls.handshake.sig_hash_sig"": ""4""
                    },
                    ""tls.handshake.sig_len"": ""256"",
                    ""tls.handshake.sig"": ""ec:89:f1:a0:ac:93:51:ad:27:0c:da:33:d2:f4:5f:8a:ee:20""
                  }
                }
              },
              ""tls.record"": {
                ""tls.record.content_type"": ""22"",
                ""tls.record.version"": ""0x0303"",
                ""tls.record.length"": ""4"",
                ""tls.handshake"": {
                  ""tls.handshake.type"": ""14"",
                  ""tls.handshake.length"": ""0""
                }
              }
            }";

            var expectedObject = new TSharkLayers
            {
                Tls = new List<TSharkTls>
                {
                    new TSharkTls
                    {
                        Records = new List<TSharkTls.TlsRecord>
                        {
                            new TSharkTls.TlsRecord
                            {
                                ContentType = "22",
                                Version = "0x0303",
                                Length = 3042,
                                Handshake = new List<TSharkTls.TlsHandshake>
                                {
                                    new TSharkTls.TlsHandshake
                                    {
                                        HandshakeType = "11",
                                        HandshakeCertificates = new TSharkTls.TlsHandshake.TlsCertificates { }
                                    }
                                },
                            }
                        }
                    },
                    new TSharkTls
                    {
                        Records = new List<TSharkTls.TlsRecord>
                        {
                            new TSharkTls.TlsRecord
                            {
                                ContentType = "22",
                                Version = "0x0303",
                                Length = 300,
                                Handshake = new List<TSharkTls.TlsHandshake>
                                {
                                    new TSharkTls.TlsHandshake { HandshakeType = "12", }
                                }
                            },
                            new TSharkTls.TlsRecord
                            {
                                ContentType = "22",
                                Version = "0x0303",
                                Length = 4,
                                Handshake = new List<TSharkTls.TlsHandshake>
                                {
                                    new TSharkTls.TlsHandshake { HandshakeType = "14", }
                                }
                            }
                        }
                    }
                }
            };


            var actualObject = JsonConvert.DeserializeObject<TSharkLayers>(_json);

            // Act & Assert
            expectedObject.Should().BeEquivalentTo(actualObject);
        }
    }


    public class TSharkFrameTests
    {
        [Fact]
        public void ShouldDeserializeTSharkFrameCorrectly()
        {
            // Arrange
            string json = @"{
              ""frame.section_number"": ""1"",
              ""frame.interface_id"": ""0"",
              ""frame.interface_id_tree"": {
                    ""frame.interface_name"": ""unknown""
              },
              ""frame.encap_type"": ""1"",
              ""frame.time"": ""Oct 16, 2023 17:02:22.438229000 Pacific Daylight Time"",
              ""frame.offset_shift"": ""0.000000000"",
              ""frame.time_epoch"": ""1697500942.438229000"",
              ""frame.time_delta"": ""0.015165000"",
              ""frame.time_delta_displayed"": ""0.015165000"",
              ""frame.time_relative"": ""0.604358000"",
              ""frame.number"": ""15"",
              ""frame.len"": ""640"",
              ""frame.cap_len"": ""640"",
              ""frame.marked"": ""0"",
              ""frame.ignored"": ""0"",
              ""frame.protocols"": ""eth: ethertype: ip: tcp: tls: http2""
            }";

            var expectedObj = new TSharkFrame
            {
                SectionNumber = 1,
                Time = "Oct 16, 2023 17:02:22.438229000 Pacific Daylight Time",
                TimeEpoch = new DateTime(2023, 10, 16, 17, 2, 22).AddTicks(4382290),
                TimeDelta = 0.015165,
                TimeRelative = 0.604358,
                Number = 15,
                Len = 640,
                Ignored = false,
                Protocols = "eth: ethertype: ip: tcp: tls: http2"
            };

            // Act
            var actualObj = JsonConvert.DeserializeObject<TSharkFrame>(json);

            // Assert
            actualObj.Should().NotBeNull();
            actualObj!.Should().BeEquivalentTo(expectedObj);
        }
    }


    public class WholeWireSharkPacketTests
    {
        [Fact]
        public void DeserializeJson_WithEmptyFields_ShouldCreateEquivalentWireSharkObject()
        {
            // Arrange
            var json = @"{
                ""_index"": ""packets-2023-10-16"",
                ""_type"": ""doc"",
                ""_score"": null,
                ""_source"": {
                  ""layers"": {
                    ""frame"": {},
                    ""eth"": {},
                    ""ip"": {},
                    ""tcp"": {},
                    ""tls"": {},
                    ""http2"": {},
                    ""http2"": {}
                  }
                }
              }";

            // Action
            var actualObj = JsonConvert.DeserializeObject<WireSharkPacket>(json);

            // Assert
            var expectedObj = new WireSharkPacket
            {
                Source = new TSharkSource
                {
                    Layers = new TSharkLayers
                    {
                        Frame = new TSharkFrame(),
                        Eth = new TSharkEth(),
                        Ip = new TSharkIp(),
                        Tcp = new TSharkTcp(),
                        Tls = new List<TSharkTls>
                        {
                            new TSharkTls(),
                        },
                        Http2 = new List<TSharkHttp2>
                        {
                            new TSharkHttp2(),
                            new TSharkHttp2(),
                        },
                        Http = null,
                    }
                }
            };

            actualObj.Should().BeEquivalentTo(expectedObj);
        }

        [Fact]
        public void DeserializeJson_WithMinimalData_ShouldCreateEquivalentWireSharkObject()
        {
            // Arrange
            string json = @"{
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
                  ""http2"": {
                    ""http2.stream"": {
                      ""http2.magic"": ""magic_string""
                    }
                  }
                }
              }
            }";

            // Act
            var actualObj = JsonConvert.DeserializeObject<WireSharkPacket>(json);

            // Assert
            var expectedObj = new WireSharkPacket
            {
                Source = new TSharkSource
                {
                    Layers = new TSharkLayers
                    {
                        Frame = new TSharkFrame { SectionNumber = 1 },
                        Eth = new TSharkEth { Dst = "00:1A:2B:3C:4D:5E" },
                        Ip = new TSharkIp { Version = "IPv4" },
                        Tcp = new TSharkTcp { SrcPort = "8080" },
                        TcpSegments = new TSharkTcpSegments { Segment = new List<int> { 1, 2 } },
                        Http = new TSharkHttp
                        {
                            ResponseLines = new List<string> { "Content-Type: text/html" },
                            Lines = new Dictionary<string, string> { ["Content-Type"] = "text/html" },
                        },
                        Http2 = new List<TSharkHttp2> { new TSharkHttp2 { Stream = new TSharkHttp2.Http2Stream { Magic = "magic_string" } } }
                    }
                }
            };

            actualObj.Should().BeEquivalentTo(expectedObj);
        }

    }


    public class TSharkHttp2Tests
    {
        [Fact]
        public void ShouldDeserializeHttp2Correctly()
        {
            // Arrange
            string json = @"{
              ""http2.stream"": {
                ""http2.length"": ""146"",
                ""http2.type"": ""1"",
                ""http2.flags"": ""0x25"",
                ""http2.flags_tree"": {
                  ""http2.flags.unused_headers"": ""0x00"",
                  ""http2.flags.priority"": ""1"",
                  ""http2.flags.padded"": ""0"",
                  ""http2.flags.eh"": ""1"",
                  ""http2.flags.end_stream"": ""1""
                },
                ""http2.r"": ""0x00000000"",
                ""http2.streamid"": ""15"",
                ""http2.pad_length"": ""0"",
                ""http2.exclusive"": ""0"",
                ""http2.stream_dependency"": ""7"",
                ""http2.headers.weight"": ""21"",
                ""http2.headers.weight_real"": ""22"",
                ""http2.headers"": ""83:35:05:b1:1f"",
                ""http2.header.length"": ""320"",
                ""http2.header.count"": ""9"",
                ""http2.header"": {
                  ""http2.header.name.length"": ""7"",
                  ""http2.header.name"": "":method"",
                  ""http2.header.value.length"": ""3"",
                  ""http2.header.value"": ""GET"",
                  ""http2.headers.method"": ""GET"",
                  ""http2.header.unescaped"": ""GET"",
                  ""http2.header.repr"": ""Indexed Header Field"",
                  ""http2.header.index"": ""2""
                },
                ""http2.header"": {
                  ""http2.header.name.length"": ""5"",
                  ""http2.header.name"": "":path"",
                  ""http2.header.value.length"": ""4"",
                  ""http2.header.value"": ""/v1/"",
                  ""http2.headers.path"": ""/v1/"",
                  ""http2.header.unescaped"": ""/v1/"",
                  ""http2.header.repr"": ""Literal Header Field without Indexing - Indexed Name"",
                  ""http2.header.index"": ""5""
                },
                ""http2.header"": {
                  ""http2.header.name.length"": ""7"",
                  ""http2.header.name"": "":scheme"",
                  ""http2.header.value.length"": ""5"",
                  ""http2.header.value"": ""https"",
                  ""http2.headers.scheme"": ""https"",
                  ""http2.header.unescaped"": ""https"",
                  ""http2.header.repr"": ""Indexed Header Field"",
                  ""http2.header.index"": ""7""
                },
                ""http2.header"": {
                  ""http2.header.name.length"": ""15"",
                  ""http2.header.name"": ""accept-encoding"",
                  ""http2.header.value.length"": ""17"",
                  ""http2.header.value"": ""gzip, deflate, br"",
                  ""http2.headers.accept_encoding"": ""gzip, deflate, br"",
                  ""http2.header.unescaped"": ""gzip, deflate, br"",
                  ""http2.header.repr"": ""Literal Header Field with Incremental Indexing - Indexed Name"",
                  ""http2.header.index"": ""16""
                },
                ""http2.request.full_uri"": ""https://firefox.settings.services.mozilla.com/v1/""
              }
            }";

            var expectedObject = new TSharkHttp2
            {
                Stream = new TSharkHttp2.Http2Stream
                {
                    Type = "1",
                    Flags = "0x25",
                    StreamId = 15,
                    Header = new List<TSharkHttp2.TSharkHttp2Header>
                    {
                        new TSharkHttp2.TSharkHttp2Header
                        {
                            Name = ":method",
                            Value = "GET"
                        },
                        new TSharkHttp2.TSharkHttp2Header
                        {
                            Name = ":path",
                            Value = "/v1/"
                        },
                        new TSharkHttp2.TSharkHttp2Header
                        {
                            Name = ":scheme",
                            Value = "https"
                        },
                        new TSharkHttp2.TSharkHttp2Header
                        {
                            Name = "accept-encoding",
                            Value = "gzip, deflate, br"
                        }
                    },
                    FullUri = "https://firefox.settings.services.mozilla.com/v1/"
                }
            };

            // Act
            var result = JsonConvert.DeserializeObject<TSharkHttp2>(json);

            // Assert
            result.Should().BeEquivalentTo(expectedObject);
        }
    }

}
