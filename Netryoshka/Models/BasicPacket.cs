using Netryoshka.Services;
using System;
using System.Net;
using System.Net.NetworkInformation;

namespace Netryoshka
{
    public class BasicPacket
    {
        public FlowEndpoint SrcEndpoint { get; }
        public FlowEndpoint DstEndpoint { get; }
        public FlowKey FlowKey { get; }
        public byte[] Payload { get; }
        public DateTime Timestamp { get; }
        public BPProtocol Protocol { get; }
        public TcpHeaders? TcpHeadersData { get; }
        public TcpProcessRecord? ProcessInfo { get; set; }
        public BPDirection Direction { get; set; }


        public BasicPacket(
            IPAddress sourceAddress, ushort sourcePort,
            IPAddress destinationAddress, ushort destinationPort,
            byte[] payload, DateTime timestamp,
            BPProtocol protocol,
            TcpHeaders? tcpHeaders = null,
            PhysicalAddress? sourceMacAddress = null,
            PhysicalAddress? destinationMacAddress = null,
            BPDirection? direction = null)
        {
            SrcEndpoint = new FlowEndpoint(sourceAddress, sourcePort, sourceMacAddress ?? PhysicalAddress.None);
            DstEndpoint = new FlowEndpoint(destinationAddress, destinationPort, destinationMacAddress ?? PhysicalAddress.None);
            FlowKey = new FlowKey(SrcEndpoint, DstEndpoint);

            Payload = payload;
            Timestamp = timestamp;
            Protocol = protocol;
            TcpHeadersData = tcpHeaders;

            // Initialize the process information to null; these can be set later.
            ProcessInfo = null;
            //ProcessId = null;
            //ProcessName = null;
            //ProcessState = null;

            Direction = direction ?? BPDirection.Unknown;
        }

        public readonly record struct TcpHeaders(
            uint SequenceNumber,
            uint AcknowledgmentNumber,
            bool IsAcknowledgment,
            bool IsSynchronize,
            bool IsFinal,
            bool IsReset,
            bool IsPush,
            bool IsUrgent,
            ushort WindowSize,
            int DataOffset,
            int TotalPacketLength,
            ushort Checksum)
        {
            public TcpRole Role() => DetermineRole();

            private TcpRole DetermineRole()
            {
                if (IsSynchronize && IsAcknowledgment) return TcpRole.SynAck;
                if (IsSynchronize) return TcpRole.Syn;
                if (IsAcknowledgment && IsFinal) return TcpRole.FinAck;
                if (IsFinal) return TcpRole.Fin;
                if (IsPush) return TcpRole.Psh;
                if (IsUrgent) return TcpRole.Urg;
                if (IsReset) return TcpRole.Rst;
                if (IsAcknowledgment) return TcpRole.Ack;
                return TcpRole.Other;
            }

        }

        public enum TcpRole
        {
            Syn,     // Synchronize sequence numbers, used when initiating a connection
            SynAck,  // Acknowledgment of a SYN request, typically the response to a SYN
            Fin,     // Finish, used for terminating a connection
            FinAck,  // Acknowledgment of a FIN request, typically the response to a FIN
            Psh,     // Push and ignore buffer wait, typically indicates end of data sending (usually pshAck)
            Urg,     // Urgent (usually urgAck)
            Ack,     // Acknowledgment, typically sent to acknowledge receipt of a packet
            Rst,     // Reset, used to abort a connection in response to an error
                     //Null,    // A packet with no flags set. This is rare but could be used for probing.
                     //Cwr,     // Congestion Window Reduced
                     //Ece,     // Explicit Congestion Notification Echo, to indicate network congestion.
            Other    // For cases when none of the well-known flag combinations apply 
        }

        public enum BPDirection
        {
            Incoming,
            Outgoing,
            Unknown
        }

        public enum BPProtocol
        {
            TCP,
            UDP,
            Other
        }
    }

    
}
