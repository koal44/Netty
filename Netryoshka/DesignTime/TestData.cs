using Netty.Services;
using Netty.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using static Netty.BasicPacket;

namespace Netty.DesignTime
{
    public static class TestData
    {

        public static TcpHeaders CreateTcpHeaderFields(TcpRole role)
        {
            return role switch
            {
                TcpRole.Syn => new TcpHeaders(
                        SequenceNumber: 1,
                        AcknowledgmentNumber: 0,
                        IsAcknowledgment: false,
                        IsSynchronize: true,
                        IsFinal: false,
                        IsReset: false,
                        IsPush: false,
                        IsUrgent: false,
                        WindowSize: 512,
                        DataOffset: 0,
                        TotalPacketLength: 64,
                        Checksum: 0
                    ),
                TcpRole.Ack => new TcpHeaders(
                        SequenceNumber: 1000,
                        AcknowledgmentNumber: 1001,
                        IsAcknowledgment: true,
                        IsSynchronize: false,
                        IsFinal: false,
                        IsReset: false,
                        IsPush: false,
                        IsUrgent: false,
                        WindowSize: 512,
                        DataOffset: 0,
                        TotalPacketLength: 64,
                        Checksum: 0
                    ),
                TcpRole.Fin => new TcpHeaders(
                        SequenceNumber: 2000,
                        AcknowledgmentNumber: 2001,
                        IsAcknowledgment: false,
                        IsSynchronize: false,
                        IsFinal: true,
                        IsReset: false,
                        IsPush: false,
                        IsUrgent: false,
                        WindowSize: 512,
                        DataOffset: 0,
                        TotalPacketLength: 64,
                        Checksum: 0
                                    ),
                _ => throw new ArgumentException($"Role '{role}' is not supported by this factory."),
            };
        }


        private static BasicPacket CreateOutgoingPacketOnFlowKey(FlowKey flowkey, string hex, DateTime dateTime, TcpHeaders tcpHeader)
        {
            return new BasicPacket(flowkey.Endpoint1.IpAddress,
                flowkey.Endpoint1.Port,
                flowkey.Endpoint2.IpAddress,
                flowkey.Endpoint2.Port,
                Util.HexToBytes(hex),
                dateTime,
                BPProtocol.TCP,
                tcpHeader,
                flowkey.Endpoint1.MacAddress,
                flowkey.Endpoint2.MacAddress,
                BPDirection.Outgoing);
        }

        private static BasicPacket CreateIncomingPacketOnFlowKey(FlowKey flowkey, string hex, DateTime dateTime, TcpHeaders tcpHeader)
        {
            return new BasicPacket(flowkey.Endpoint2.IpAddress,
                flowkey.Endpoint2.Port,
                flowkey.Endpoint1.IpAddress,
                flowkey.Endpoint1.Port,
                Util.HexToBytes(hex),
                dateTime,
                BPProtocol.TCP,
                tcpHeader,
                flowkey.Endpoint2.MacAddress,
                flowkey.Endpoint1.MacAddress,
                BPDirection.Incoming);
        }

        public static List<BasicPacket> GetPackets()
        {
            var endpoint1 = new FlowEndpoint(
                IPAddress.Parse("192.168.1.1"),
                1111,
                new PhysicalAddress(new byte[] { 0x00, 0x1A, 0x2B, 0x3C, 0x4D, 0x5E }));
            var endpoint2 = new FlowEndpoint(IPAddress.Parse("192.168.1.2"),
                80,
                new PhysicalAddress(new byte[] { 0x5E, 0x4D, 0x3C, 0x2B, 0x1A, 0x00 }));

            var flowKey = new FlowKey(endpoint1, endpoint2);
            var now = DateTime.Now;
            var synHeader = CreateTcpHeaderFields(TcpRole.Syn);
            var ackHeader = CreateTcpHeaderFields(TcpRole.Ack);

            return new List<BasicPacket>()
            {
                CreateOutgoingPacketOnFlowKey(
                    flowKey,
                    Util.GenerateRandomHexString(6),
                    now.AddMilliseconds(10),
                    synHeader),
                CreateIncomingPacketOnFlowKey(flowKey,
                    Util.GenerateRandomHexString(166),
                    now.AddMilliseconds(50),
                    ackHeader),
                CreateOutgoingPacketOnFlowKey(flowKey,
                    Util.GenerateRandomHexString(6),
                    now.AddMilliseconds(200),
                    ackHeader),
                CreateOutgoingPacketOnFlowKey(flowKey,
                    Util.GenerateRandomHexString(16),
                    now.AddMilliseconds(500),
                    ackHeader),
                CreateIncomingPacketOnFlowKey(flowKey,
                    Util.GenerateRandomHexString(6),
                    now.AddMilliseconds(1234),
                    ackHeader),
                CreateOutgoingPacketOnFlowKey(flowKey,
                    Util.GenerateRandomHexString(226),
                    now.AddMilliseconds(555555),
                    ackHeader)
            };
        }


    }
}
