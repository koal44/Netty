using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace Netty.Services
{
    public interface ICaptureService
    {
        bool HasData { get; }
        int CapturedPacketCount { get; }

        List<string> GetDeviceNames();
        string? GetSelectedDeviceName();
        bool IsValidFilter(string filter, string deviceName);
        void LoadPacketsFromFile(Action<BasicPacket> packetReceivedCallback, string fileName);
        bool RequiresAdminPrivileges(string deviceName);
        public Task StartCaptureAsync(Action<BasicPacket> packetReceivedCallback, FilterData filterdata, string deviceName, bool isPromiscuous = false);
        void StopCapture();
        void WritePacketsToFile(string fileName);
        void WritePacketsToFile(string dummyFilePath, IEnumerable<BasicPacket> packets);
    }
}
