using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Netryoshka.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using static Netryoshka.TSharkHttp;

namespace Netryoshka
{
    public class WireSharkPacket
    {
        //[JsonProperty("_index")]
        //public string Index { get; set; }

        //[JsonProperty("_type")]
        //public string EthType { get; set; }

        //[JsonProperty("_score")]
        //public double? Score { get; set; }

        [JsonProperty("_source")]
        public TSharkSource? Source { get; set; }
    }

    public class TSharkSource
    {
        [JsonProperty("layers")]
        public TSharkLayers? Layers { get; set; }
    }

    public class TSharkLayers
    {
        [JsonProperty("frame")]
        public TSharkFrame? Frame { get; set; }

        [JsonProperty("eth")]
        public TSharkEth? Eth { get; set; }

        [JsonProperty("ip")]
        public TSharkIp? Ip { get; set; }

        [JsonProperty("tcp")]
        public TSharkTcp? Tcp { get; set; }

        [JsonProperty("http")]
        public TSharkHttp? Http { get; set; }

        [JsonProperty("data")]
        public TSharkData? Data { get; set; }
    }

    public class TSharkFrame
    {
        // Section number within the packet capture.
        [JsonProperty("frame.section_number")]
        public uint? SectionNumber { get; set; }

        // Time of frame capture in human-readable format.
        [JsonProperty("frame.time")]
        public string? Time { get; set; }

        // Time of frame capture in epoch seconds.
        [JsonProperty("frame.time_epoch")]
        [JsonConverter(typeof(JsonUtil.EpochToDateTimeConverter))]
        public DateTime? TimeEpoch { get; set; }

        // Time between this and the previous frame in seconds.
        [JsonProperty("frame.time_delta")]
        public double? TimeDelta { get; set; }

        // Time elapsed since capture start in seconds.
        [JsonProperty("frame.time_relative")]
        public double? TimeRelative { get; set; }

        // Frame's sequence number.
        [JsonProperty("frame.number")]
        public uint? Number { get; set; }

        // Frame length in bytes.
        [JsonProperty("frame.len")]
        public uint? Len { get; set; }

        // Whether the frame is ignored.
        [JsonProperty("frame.ignored")]
        [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
        public bool? Ignored { get; set; }

        // List of protocols involved in the frame.
        [JsonProperty("frame.protocols")]
        public string? Protocols { get; set; }
    }



    public class TSharkEth
    {
        [JsonProperty("eth.dst")]
        public string? Dst { get; set; }

        [JsonProperty("eth.src")]
        public string? Src { get; set; }

        [JsonProperty("eth.dst_tree")]
        public EthernetDstTree? DstTree { get; set; }

        [JsonProperty("eth.src_tree")]
        public EthernetSrcTree? SrcTree { get; set; }

        [JsonProperty("eth.type")]
        public string? EthType { get; set; }

        public string? EthTypeVal { get; set; }


        public class EthernetDstTree
        {
            [JsonProperty("eth.dst.oui_resolved")]
            public string? DstOuiResolved { get; set; }
        }

        public class EthernetSrcTree
        {
            [JsonProperty("eth.src.oui_resolved")]
            public string? SrcOuiResolved { get; set; }
        }


        public static readonly Dictionary<int, string> EthTypeToProtocol = new()
        {
            { 0x0000, "ETHERTYPE_UNK" },
            { 0x0600, "XNS Internet Datagram Protocol" },
            { 0x0800, "IPv4" },
            { 0x0805, "X.25 Layer 3" },
            { 0x0806, "ARP" },
            { 0x0842, "Wake on LAN" },
            { 0x08F0, "WiMax Mac-to-Mac" },
            { 0x08FF, "AX.25" },
            { 0x0BAD, "Vines IP" },
            { 0x0BAF, "Vines Echo" },
            { 0x0C15, "ETHERTYPE_C15_HBEAT" },
            { 0x1984, "Netmon Train" },
            { 0x2001, "Cisco Group Management Protocol" },
            { 0x22E5, "Gigamon Header" },
            { 0x22EA, "802.1Qat Multiple Stream Reservation Protocol" },
            { 0x22F0, "IEEE 1722 Audio Video Transport Protocol" },
            { 0x22F1, "Robust Header Compression(RoHC)" },
            { 0x22F3, "Transparent Interconnection of Lots of Links" },
            { 0x22F4, "Intermediate System to Intermediate System" },
            { 0x2452, "IEEE 802.11 (Centrino promiscuous)" },
            { 0x3C07, "3Com NBP Datagram" },
            { 0x3E3F, "EPL_V1" },
            { 0x4742, "ETHERTYPE_C15_CH" },
            { 0x6000, "DEC proto" },
            { 0x6001, "DEC DNA Dump/Load" },
            { 0x6002, "DEC DNA Remote Console" },
            { 0x6003, "DEC DNA Routing" },
            { 0x6004, "DEC LAT" },
            { 0x6005, "DEC Diagnostics" },
            { 0x6006, "DEC Customer use" },
            { 0x6007, "DEC LAVC/SCA" },
            { 0x6558, "Transparent Ethernet bridging" },
            { 0x6559, "ETHERTYPE_RAW_FR" },
            { 0x8035, "RARP" },
            { 0x8038, "DEC LanBridge" },
            { 0x8041, "DEC LAST" },
            { 0x809B, "AppleTalk LLAP bridging" },
            { 0x80D5, "SNA-over-Ethernet" },
            { 0x80E1, "EtherNet/IP Device Level Ring" },
            { 0x80F3, "AARP" },
            { 0x8100, "802.1Q Virtual LAN" },
            { 0x8102, "Simple Loop Protection Protocol" },
            { 0x8103, "Virtual LACP" },
            { 0x8104, "Simple Loop Protection Protocol (old)" },
            { 0x8133, "Juniper Netscreen Redundant Protocol" },
            { 0x8137, "Netware IPX/SPX" },
            { 0x814C, "ETHERTYPE_SNMP" },
            { 0x80FF, "Wellfleet Compression Protocol" },
            { 0x8181, "Spanning Tree Protocol" },
            { 0x81FD, "Cabletron Interswitch Message Protocol" },
            { 0x81FF, "Cabletron SFVLAN 1.8 Tag-Based Flood" },
            { 0x8204, "QNX 6 QNET protocol" },
            { 0x86DD, "IPv6" },
            { 0x872D, "Cisco Wireless Lan Context Control Protocol" },
            { 0x8783, "Motorola Media Independent Network Transport" },
            { 0x8808, "MAC Control" },
            { 0x8809, "Slow Protocols" },
            { 0x880B, "ETHERTYPE_PPP" },
            { 0x8819, "Cirrus Cobranet Packet" },
            { 0x8847, "MPLS label switched packet" },
            { 0x8848, "MPLS multicast label switched packet" },
            { 0x885A, "Foundry proprietary" },
            { 0x8863, "PPPoE Discovery" },
            { 0x8864, "PPPoE Session" },
            { 0x886C, "HomePNA, wlan link local tunnel" },
            { 0x886D, "Intel ANS probe" },
            { 0x886F, "MS NLB heartbeat" },
            { 0x8870, "Jumbo LLC" },
            { 0x8874, "Broadcom tag" },
            { 0x887B, "Homeplug" },
            { 0x8881, "CDMA2000 A10 Unstructured byte stream" },
            { 0x8884, "ATM over Ethernet" },
            { 0x888E, "802.1X Authentication" },
            { 0x8890, "Fortinet FGCP (FortiGate Cluster Protocol) HB (HeartBeat)" },
            { 0x8892, "PROFINET" },
            { 0x8899, "Realtek Layer 2 Protocols" },
            { 0x889A, "ETHERTYPE_HYPERSCSI" },
            { 0x889B, "CSM_ENCAPS Protocol" },
            { 0x88A1, "Telkonet powerline" },
            { 0x88A2, "ATA over Ethernet" },
            { 0x88A4, "EtherCAT frame" },
            { 0x88A8, "802.1ad Provider Bridge (Q-in-Q)" },
            { 0x88A9, "ETHERTYPE_IEEE_EXTREME_MESH" },
            { 0x88AB, "ETHERNET Powerlink v2" },
            { 0x88AD, "XiMeta Technology" },
            { 0x88AE, "ETHERTYPE_BRDWALK" },
            { 0x88B4, "WAI Authentication Protocol" },
            { 0x88B5, "Local Experimental Ethertype 1" },
            { 0x88B6, "Local Experimental Ethertype 2" },
            { 0x88B7, "IEEE 802a OUI Extended Ethertype" },
            { 0x88B8, "IEC 61850/GOOSE" },
            { 0x88B9, "IEC 61850/GSE management services" },
            { 0x88BA, "IEC 61850/SV (Sampled Value Transmission" },
            { 0x88CA, "Transparent Inter Process Communication" },
            { 0x88C7, "802.11i Pre-Authentication" },
            { 0x88CC, "802.1 Link Layer Discovery Protocol (LLDP)" },
            { 0x88CD, "ETHERTYPE_SERCOS" },
            { 0x88D2, "CDMA2000 A10 3GPP2 Packet" },
            { 0x88D8, "Circuit Emulation Services over Ethernet (MEF8)" },
            { 0x88D9, "Link Layer Topology Discovery (LLTD)" },
            { 0x88DC, "(WAVE) Short Message Protocol (WSM)" },
            { 0x88DE, "VMware Lab Manager" },
            { 0x88E1, "Homeplug AV" },
            { 0x88E3, "MRP" },
            { 0x88E5, "802.1AE (MACsec)" },
            { 0x88E7, "802.1ah Provider Backbone Bridge (mac-in-mac)" },
            { 0x88EE, "Ethernet Local Management Interface (MEF16)" },
            { 0x88F5, "ETHERTYPE_MVRP" },
            { 0x88F6, "802.1ak Multiple Mac Registration Protocol" },
            { 0x88F7, "PTPv2 over Ethernet (IEEE1588)" },
            { 0x88F8, "Network Controller Sideband Interface" },
            { 0x88FB, "Parallel Redundancy Protocol (PRP) and HSR Supervision (IEC62439 Part 3)" },
            { 0x8901, "Flow Layer Internal Protocol" },
            { 0x8902, "IEEE 802.1Q Connectivity Fault Management (CFM) protocol" },
            { 0x8903, "Data Center Ethernet (DCE) protocol(Cisco)" },
            { 0x8906, "Fibre Channel over Ethernet" },
            { 0x8909, "CiscoMetaData" },
            { 0x890d, "IEEE 802.11 data encapsulation" },
            { 0x8911, "LINX IPC Protocol" },
            { 0x8914, "FCoE Initialization Protocol" },
            { 0x8915, "RDMA over Converged Ethernet" },
            { 0x8917, "Media Independent Handover Protocol" },
            { 0x891D, "TTEthernet Protocol Control Frame" },
            { 0x8926, "VN-Tag" },
            { 0x892B, "Schweitzer Engineering Labs Layer 2 Protocol" },
            { 0x892D, "bluecom Protocol" },
            { 0x892F, "High-availability Seamless Redundancy (IEC62439 Part 3)" },
            { 0x893A, "1905.1a Convergent Digital Home Network for Heterogenous Technologies" },
            { 0x893F, "802.1br Bridge Port Extension E-Tag" },
            { 0x8940, "ETHERTYPE_ECP" },
            { 0x8942, "ETHERTYPE_ONOS" },
            { 0x8947, "GeoNetworking" },
            { 0x894F, "Network Service Header" },
            { 0x8988, "PA HB Backup" },
            { 0x9000, "Loopback" },
            { 0x9021, "Real-Time Media Access Control" },
            { 0x9022, "Real-Time Configuration Protocol" },
            { 0x9100, "QinQ: old non-standard 802.1ad" },
            { 0x9104, "EERO Broadcast Packet" },
            { 0x99FE, "Technically Enhanced Capture Module Protocol (TECMP) or ASAM Capture Module Protocol (CMP)" },
            { 0xA0ED, "6LoWPAN" },
            { 0xAEFE, "eCPRI" },
            { 0xB4E3, "CableLabs Layer-3 Protocol" },
            { 0xC0DE, "eXpressive Internet Protocol" },
            { 0xC0DF, "Neighborhood Watch Protocol" },
            { 0xCAFE, "Veritas Low Latency Transport (not officially registered)" },
            { 0xD00D, "Digium TDM over Ethernet Protocol" },
            { 0xD28B, "Arista Vendor Specific Protocol" },
            { 0xE555, "EXos internal Extra Header" },
            { 0xFCFC, "ETHERTYPE_FCFT" },
            { 0xFFF2, "Cisco ACI ARP gleaning" },
            { 0xF1C1, "802.1CB Frame Replication and Elimination for Reliability" }
        };


        [OnDeserialized]
        internal void SetEthTypeVal(StreamingContext context)
        {
            int? ethyTypeNum = !string.IsNullOrEmpty(EthType) ? Convert.ToInt32(EthType, 16) : null;
            if (ethyTypeNum.HasValue && EthTypeToProtocol.TryGetValue(ethyTypeNum.Value, out var protocol))
            {
                EthTypeVal = protocol;
            }
            else
            {
                EthTypeVal = null;
            }
        }
    }


    public class TSharkIp
    {
        [JsonProperty("ip.version")]
        public string? Version { get; set; }

        [JsonProperty("ip.src")]
        public string? Src { get; set; }

        [JsonProperty("ip.dst")]
        public string? Dst { get; set; }

        [JsonProperty("ip.ttl")]
        public string? Ttl { get; set; }

        [JsonProperty("ip.proto")]
        public string? Proto { get; set; }

        [JsonProperty("ip.flags")]
        public string? Flags { get; set; }

        //public bool FlagsRb => (Convert.ToByte(Flags, 16) & 0x01) != 0;
        //public bool FlagsDf => (Convert.ToByte(Flags, 16) & 0x02) != 0;
        //public bool FlagsMf => (Convert.ToByte(Flags, 16) & 0x04) != 0;
    }


    public class TSharkTcp
    {
        [JsonProperty("tcp.srcport")]
        public string? SrcPort { get; set; }

        [JsonProperty("tcp.dstport")]
        public string? DstPort { get; set; }

        [JsonProperty("tcp.stream")]
        public string? Stream { get; set; }

        [JsonProperty("tcp.len")]
        public string? Len { get; set; }

        [JsonProperty("tcp.seq")]
        public string? Seq { get; set; }

        [JsonProperty("tcp.seq_raw")]
        public string? SeqRaw { get; set; }

        [JsonProperty("tcp.hdr_len")]
        public string? HdrLen { get; set; }

        [JsonProperty("tcp.flags_tree")]
        public TSharkTcpFlags? Flags { get; set; }

        [JsonProperty("tcp.window_size")]
        public string? WindowSize { get; set; }

        [JsonProperty("tcp.checksum")]
        public string? Checksum { get; set; }

        [JsonProperty("tcp.checksum.status")]
        [JsonConverter(typeof(StatusToDescriptionConverter))]
        public string? ChecksumStatus { get; set; }


        [JsonProperty("tcp.payload")]
        public string? Payload { get; set; }

        [JsonProperty("tcp.segments")]
        public TSharkSegments? Segments { get; set; }

        public class TSharkTcpFlags
        {
            [JsonProperty("tcp.flags.res")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Res { get; set; }

            [JsonProperty("tcp.flags.ae")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Ae { get; set; }

            [JsonProperty("tcp.flags.cwr")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Cwr { get; set; }

            [JsonProperty("tcp.flags.ece")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Ece { get; set; }

            [JsonProperty("tcp.flags.urg")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Urg { get; set; }

            [JsonProperty("tcp.flags.ack")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Ack { get; set; }

            [JsonProperty("tcp.flags.push")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Push { get; set; }

            [JsonProperty("tcp.flags.reset")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Reset { get; set; }

            [JsonProperty("tcp.flags.syn")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Syn { get; set; }

            [JsonProperty("tcp.flags.fin")]
            [JsonConverter(typeof(JsonUtil.BitToBoolConverter))]
            public bool Fin { get; set; }
        }

        public class TSharkSegments
        {
            [JsonProperty("tcp.segment")]
            public List<int>? Segment { get; set; }

            [JsonProperty("tcp.segment.count")]
            public int? SegmentCount { get; set; }

            [JsonProperty("tcp.reassembled.length")]
            public int? ReassembledLength { get; set; }

            [JsonProperty("tcp.reassembled.data")]
            public string? ReassembledData { get; set; }
        }

        //public static readonly Dictionary<int, string> ChecksumStatusToDescription = new()
        //{
        //    { 0, "Bad" },
        //    { 1, "Good" },
        //    { 2, "Unverified" },
        //    { 3, "Not present" },
        //    { 4, "Illegal" },
        //};

        public static readonly ReadOnlyDictionary<int, string> ChecksumStatusToDescription = new(
            new Dictionary<int, string>
            {
                { 0, "Bad" },
                { 1, "Good" },
                { 2, "Unverified" },
                { 3, "Not present" },
                { 4, "Illegal" },
            }
        );

        public class StatusToDescriptionConverter : JsonConverter<string>
        {
            public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (reader.Value == null) return null;

                try
                {
                    int key = int.Parse(reader.Value.ToString()!);
                    return ChecksumStatusToDescription[key];
                }
                catch (FormatException)
                {
                    throw new JsonSerializationException($"Invalid format: {reader.Value} cannot be converted to int.");
                }
                catch (KeyNotFoundException)
                {
                    throw new JsonSerializationException($"Invalid key: {reader.Value} is not a valid checksum status key.");
                }
            }


            //public override string? ReadJson(JsonReader reader, Type objectType, string? existingValue, bool hasExistingValue, JsonSerializer serializer)
            //{
            //    if (int.TryParse(reader.Value?.ToString(), out int key))
            //    {
            //        return ChecksumStatusToDescription.TryGetValue(key, out var status) ? status : null;
            //    }
            //    return null;
            //}

            public override void WriteJson(JsonWriter writer, string? value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
                //throw new NotImplementedException("StatusToDescriptionConverter.WriteJson()");
            }
        }
    }

   

    //public class FlagsConverter : JsonConverter
    //{
    //    public override bool CanConvert(EthType objectType)
    //    {
    //        return (objectType == typeof(TSharkTcpFlags));
    //    }

    //    public override object ReadJson(JsonReader reader, EthType objectType, object? existingValue, JsonSerializer serializer)
    //    {
    //        var jsonObject = JObject.Load(reader);
    //        var flags = new TSharkTcpFlags();

    //        foreach (var property in jsonObject.Properties())
    //        {
    //            bool flagValue = property.Value.ToString() == "1" ? true : false;

    //            switch (property.Name)
    //            {
    //                case "tcp.flags.res": flags.Res = flagValue; break;
    //                case "tcp.flags.ae": flags.Ae = flagValue; break;
    //                case "tcp.flags.cwr": flags.Cwr = flagValue; break;
    //                case "tcp.flags.ece": flags.Ece = flagValue; break;
    //                case "tcp.flags.urg": flags.Urg = flagValue; break;
    //                case "tcp.flags.ack": flags.Ack = flagValue; break;
    //                case "tcp.flags.push": flags.Push = flagValue; break;
    //                case "tcp.flags.reset": flags.Reset = flagValue; break;
    //                case "tcp.flags.syn": flags.Syn = flagValue; break;
    //                case "tcp.flags.fin": flags.Fin = flagValue; break;
    //                default: break;
    //            }
    //        }

    //        return flags;
    //    }

    //    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}



    [JsonConverter(typeof(TSharkHttpConverter))]
    public class TSharkHttp
    {
        public string? Declaration { get; set; }
        public IReadOnlyDictionary<string, string>? Lines { get; set; }
        public TSharkRequest? Request { get; set; }
        public TSharkResponse? Response { get; set; }

        public class TSharkRequest
        {
            public uint? RequestNumber { get; set; }
            public string? FullUri { get; set; }
            public uint? PrevRequestIn { get; set; }
            public uint? ResponseIn { get; set; } // response in frame. ? - doesn't seem to exist
        }

        public class TSharkResponse
        {
            public ulong? ContentLength { get; set; }
            public uint? ResponseNumber { get; set; }
            public double? Time { get; set; } // time since request
            public uint? RequestIn { get; set;   } // request in frame
            public uint? PrevRequestIn { get; set; } // previous request in frame
            public uint? PrevResponseIn { get; set; } // previous response in frame
            public string? ResponseForUri { get; set; }
            public string? FileData { get; set; }
        }
        
    }


    public class TSharkHttpConverter : JsonConverter<TSharkHttp>
    {
        public override TSharkHttp ReadJson(JsonReader reader, Type objectType, TSharkHttp? existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader is not JsonTextReader jsonTextReader)
            {
                throw new InvalidOperationException("Expected a JsonTextReader.");
            }

            var jToken = JsonUtil.DeserializeAndCombineDuplicates(jsonTextReader);
            var jsonObject = jToken as JObject ?? throw new JsonException("Expected a JSON object.");

            return TSharkHttpFactory.CreateFromJson(jsonObject);
        }

        // Use the default serialization
        // public override bool CanWrite => false; 

        public override void WriteJson(JsonWriter writer, TSharkHttp? value, JsonSerializer serializer)
        {
            //throw new InvalidOperationException("WriteJson should not be called.");

            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartObject();

            // Serialize Declaration
            writer.WritePropertyName("Declaration");
            serializer.Serialize(writer, value.Declaration);

            // Serialize Lines
            writer.WritePropertyName("Lines");
            serializer.Serialize(writer, value.Lines);

            // Serialize Request
            writer.WritePropertyName("Request");
            serializer.Serialize(writer, value.Request);

            // Serialize Response
            writer.WritePropertyName("Response");
            serializer.Serialize(writer, value.Response);

            writer.WriteEndObject();
        }

    }

    public static class TSharkHttpFactory
    {
        public static TSharkHttp CreateFromJson(JObject jsonObject)
        {
            if (jsonObject == null || !jsonObject.HasValues)
            {
                throw new ArgumentException("Invalid JSON object.", nameof(jsonObject));
            }

            return new TSharkHttp
            {
                Declaration = GetDeclaration(jsonObject),
                Lines = GetLineDictionary(jsonObject),
                Request = CreateRequest(jsonObject),
                Response = CreateResponse(jsonObject)
            };
        }

        private static string GetDeclaration(JObject jsonObject)
        {
            // Take the first property's name as the message
            var firstProperty = jsonObject.Properties().FirstOrDefault();
            return firstProperty?.Name ?? string.Empty;
        }

        private static IReadOnlyDictionary<string, string> GetLineDictionary(JObject jsonObject)
        {
            var dictResult = new Dictionary<string, string>();
            foreach (var property in jsonObject.Properties())
            {
                if (property.Name.Equals("http.response.line") || property.Name.Equals("http.request.line"))
                {
                    foreach (var line in property.Value?.Children() ?? Enumerable.Empty<JToken>())
                    {
                        var lineStr = line?.ToString();
                        if (string.IsNullOrEmpty(lineStr)) continue;

                        var parts = lineStr.Split(new[] { ": " }, StringSplitOptions.None);
                        if (parts.Length >= 2)
                        {
                            dictResult[parts[0]] = string.Join(": ", parts.Skip(1));
                        }
                    }
                }
            }

            return new ReadOnlyDictionary<string, string>(dictResult);
        }

        private static TSharkRequest? CreateRequest(JObject jsonObject)
        {
            return JsonUtil.JObjToBool(jsonObject["http.request"])
                ? new TSharkRequest()
                {
                    RequestNumber = JsonUtil.JObjToValue<uint>(jsonObject["http.request_number"]),
                    FullUri = JsonUtil.JObjToString(jsonObject["http.request.full_uri"]),
                    PrevRequestIn = JsonUtil.JObjToValue<uint>(jsonObject["http.prev_request_in"]),
                    ResponseIn = JsonUtil.JObjToValue<uint>(jsonObject["http.response_in"]),
                }
                : null;
        }

        private static TSharkResponse? CreateResponse(JObject jsonObject)
        {
            return JsonUtil.JObjToBool(jsonObject["http.response"])
                ? new TSharkResponse()
                {
                    ContentLength = JsonUtil.JObjToValue<ulong>(jsonObject["http.content_length_header"]),
                    ResponseNumber = JsonUtil.JObjToValue<uint>(jsonObject["http.response_number"]),
                    Time = JsonUtil.JObjToValue<double>(jsonObject["http.time"]),
                    RequestIn = JsonUtil.JObjToValue<uint>(jsonObject["http.request_in"]),
                    PrevRequestIn = JsonUtil.JObjToValue<uint>(jsonObject["http.prev_request_in"]),
                    PrevResponseIn = JsonUtil.JObjToValue<uint>(jsonObject["http.prev_response_in"]),
                    ResponseForUri = JsonUtil.JObjToString(jsonObject["http.response_for.uri"]),
                    FileData = JsonUtil.JObjToString(jsonObject["http.file_data"]),
                }
                : null;
        }

    }






    public class TSharkData
    {
        // Define properties relevant to 'data'
    }


}
