using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace Proton.Retire
{
    public class DataStreamRepository
    {
        private readonly List<DataStream> dataStreams = new();

        private int iggPort = -1;
        public int IggPort
        {
            get => iggPort;
            set
            {
                dataStreams.Clear();
                iggPort = value;
            }
        }

        private static DataStreamRepository? instance;
        public static DataStreamRepository Instance
        {
            get => instance ??= new DataStreamRepository();
        }

        private DataStreamRepository() { }

        public void AddData(int srcPort, int dstPort, IPAddress srcAddr, IPAddress dstAddr, byte[] data, DateTime timeVal)
        {
            if (IggPort < 0)
            {
                throw new Exception("IggPort was not set");
            }

            int localPort = srcPort == IggPort ? dstPort : srcPort;
            var localAddr = srcPort == IggPort ? dstAddr : srcAddr;
            var iggAddr = srcPort == IggPort ? srcAddr : dstAddr;

            var dataStream = dataStreams.FirstOrDefault(x =>
                localPort == x.localPort
                && IggPort == x.iggPort
                && (srcAddr.Equals(x.localAddr) || srcAddr.Equals(x.iggAddr))
                && (dstAddr.Equals(x.localAddr) || dstAddr.Equals(x.iggAddr))
                );
            if (dataStream == null)
            {
                dataStream = new DataStream(IggPort, localPort, iggAddr, localAddr);
                dataStreams.Add(dataStream);
            }
            dataStream.AddData(srcPort, dstPort, data, timeVal);
        }

        public void Clear()
        {
            dataStreams.Clear();
        }

        public DataStream? GetDataStream(int localPort)
        {
            var dataStream = dataStreams.FirstOrDefault(x => x.localPort == localPort);

            return dataStream;
        }

        public List<int>? GetPorts()
        {
            var hash = new HashSet<int>();
            foreach (var dataStream in dataStreams)
            {
                hash.Add(dataStream.localPort);
            }
            return hash.OrderBy(x => x).ToList();
        }
    }
}
