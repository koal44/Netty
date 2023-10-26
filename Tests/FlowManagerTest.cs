using FluentAssertions;
using Netryoshka;
using Netryoshka.Services;
using System.Net;
using System.Net.NetworkInformation;
using Xunit;
using static Netryoshka.BasicPacket;

namespace Tests
{
    public class FlowManagerTests
    {
        private static BasicPacket CreatePacket(IPAddress sourceAddress, ushort sourcePort, IPAddress destAddress, ushort destPort, byte[]? payload = null, DateTime? timestamp = null, BPProtocol? protocolType = null)
            => new(
                sourceAddress, sourcePort,
                destAddress, destPort,
                payload ?? Array.Empty<byte>(),
                timestamp ?? DateTime.Now,
                protocolType ?? BPProtocol.TCP);


        [Fact]
        public void ShouldGroupPacketsByFlows()
        {
            // Arrange
            var service = new FlowManager();

            var packet1 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("8.8.8.8"), 99);

            var packet2 = CreatePacket(
                IPAddress.Parse("2.2.2.2"), 33,
                IPAddress.Parse("8.8.8.8"), 99);

            var packet3 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("8.8.8.8"), 99);

            // Act
            service.AddPacket(packet1);
            service.AddPacket(packet2);
            service.AddPacket(packet3);

            // Assert
            var allFlows = service.GetAllFlows();
            allFlows.Should().HaveCount(2); // Two unique flows

            allFlows[packet1.FlowKey].Should().HaveCount(2); // Two packets in the first flow
            allFlows[packet2.FlowKey].Should().HaveCount(1); // One packet in the second flow
        }


        [Fact]
        public void ShouldGroupIntoSameFlowDespiteDirection()
        {
            // Arrange
            var service = new FlowManager();

            var packet1 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("8.8.8.8"), 99);

            var packet2 = CreatePacket(
                IPAddress.Parse("8.8.8.8"), 99,
                IPAddress.Parse("1.1.1.1"), 22);

            var packet3 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("8.8.8.8"), 99);

            // Act
            service.AddPacket(packet1);
            service.AddPacket(packet2);
            service.AddPacket(packet3);

            // Assert
            var allFlows = service.GetAllFlows();
            allFlows.Should().HaveCount(1); // One unique flow

            // Since all three packets are part of the same flow, 
            // the length of the list for that flow should be 3.
            allFlows[packet1.FlowKey].Should().HaveCount(3);
        }


        [Fact]
        public void ShouldHandleDuplicatePacketsInSameFlow()
        {
            // Arrange
            var service = new FlowManager();

            var packet1 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("8.8.8.8"), 99);

            // Act
            service.AddPacket(packet1);
            service.AddPacket(packet1);  // Duplicate

            // Assert
            var allFlows = service.GetAllFlows();
            allFlows[packet1.FlowKey].Should().HaveCount(2); // Two packets, including the duplicate
        }


        [Fact]
        public void ShouldReturnEmptyListForUnavailableFlowEndpoint()
        {
            // Arrange
            var service = new FlowManager();

            // Act
            var packets = service.GetAllFlowsByEndpoint(new FlowEndpoint(IPAddress.Parse("3.3.3.3"), 44, PhysicalAddress.None));

            // Assert
            packets.Should().BeEmpty(); // Expecting an empty list
        }


        [Fact]
        public void ShouldRetrieveAllPacketsForGivenFlowEndpoint()
        {
            // Arrange
            var service = new FlowManager();

            var packet1 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("8.8.8.8"), 99);

            var packet2 = CreatePacket(
                IPAddress.Parse("2.2.2.2"), 33,
                IPAddress.Parse("8.8.8.8"), 99);

            var packet3 = CreatePacket(
                IPAddress.Parse("8.8.8.8"), 99,
                IPAddress.Parse("1.1.1.1"), 22);

            // Adding packet with reverse direction to test the flow consistency
            var packet4 = CreatePacket(
                IPAddress.Parse("8.8.8.8"), 99,
                IPAddress.Parse("2.2.2.2"), 33);

            var packet5 = CreatePacket(
                IPAddress.Parse("1.1.1.1"), 22,
                IPAddress.Parse("7.7.7.7"), 80);

            var packet6 = CreatePacket(
                IPAddress.Parse("3.3.3.3"), 4444,
                IPAddress.Parse("5.5.5.5"), 6666);

            // Act
            service.AddPacket(packet1);
            service.AddPacket(packet2);
            service.AddPacket(packet3);
            service.AddPacket(packet4);
            service.AddPacket(packet5);
            service.AddPacket(packet6);

            // Assert
            var endpointToQuery = packet4.FlowKey.Endpoint1;
            var packetsForEndpoint = service.GetAllFlowsByEndpoint(endpointToQuery);

            packetsForEndpoint.Should().HaveCount(4);
        }

    }
}
