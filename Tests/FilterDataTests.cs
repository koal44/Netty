using FluentAssertions;
using Netryoshka;
using System.Net;
using Xunit;
using static Netryoshka.BasicPacket;

namespace Tests
{
    public class FilterDataTests
    {
#pragma warning disable IDE0230 // Use UTF-8 string literal
#pragma warning disable CA1825 // Avoid zero-length array allocations

        [Fact]
        public void ShouldKeepPacket_RemotePortMatchesOutgoingSrc_ReturnsTrue()
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 1002 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Outgoing
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldKeepPacket_RemotePortNoMatchOutgoingSrc_ReturnsFalse()
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 1003 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Outgoing
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeFalse();
        }



        [Fact]
        public void ShouldKeepPacket_RemotePortMatchesIncomingDst_ReturnsFalse()
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 1002 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Incoming
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldKeepPacket_RemotePortMatchesUnknownSrc_ReturnsTrue()
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 1001 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Unknown
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeTrue();
        }


        [Fact]
        public void ShouldKeepPacket_EmptyRemotePorts_ReturnsFalse() // can i test all three directions within 1 test?
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Outgoing
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldKeepPacket_MultipleRemotePortsOnlyOneMatchesOutgoingSrc_ReturnsTrue()
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 999, 1000, 1002 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Outgoing
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ShouldKeepPacket_RemotePortNoMatchUnknownSrc_ReturnsFalse()
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 999, 1000 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 1001,
                IPAddress.Parse("192.168.1.2"), 1002,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Unknown
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ShouldKeepPacket_RemotePortMatchesBothSrcAndDst_ReturnsTrue() // test multiple directons?
        {
            // Assert
            var filterData = new FilterData(remotePorts: new HashSet<int> { 3333 });
            var packet = new BasicPacket(
                IPAddress.Parse("192.168.1.1"), 3333,
                IPAddress.Parse("192.168.1.2"), 3333,
                new byte[] { }, DateTime.Now,
                BPProtocol.TCP)
            {
                Direction = BPDirection.Outgoing
            };

            // Act 
            var result = filterData.ShouldKeepPacket(packet);

            // Assert
            result.Should().BeTrue();
        }
#pragma warning restore IDE0230 // Use UTF-8 string literal
#pragma warning restore CA1825 // Avoid zero-length array allocations

    }

}
