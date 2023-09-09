using System;
using System.Collections.Generic;
using System.Net;

namespace Proton.Retire
{
    public class DataStream
    {
        public readonly int iggPort;
        public readonly int localPort;
        public readonly IPAddress iggAddr;
        public readonly IPAddress localAddr;

        public List<DataWrap> incomingData;
        public List<DataWrap> outgoingData;

        public DataStream(int iggPort, int localPort, IPAddress iggAddr, IPAddress localAddr)
        {
            this.iggPort = iggPort;
            this.localPort = localPort;
            this.localAddr = localAddr;
            this.iggAddr = iggAddr;

            incomingData = new List<DataWrap>();
            outgoingData = new List<DataWrap>();
        }

        public void AddData(int srcPort, int dstPort, byte[] data, DateTime timeVal)
        {
            if (srcPort != iggPort && srcPort != localPort || dstPort != iggPort && dstPort != localPort)
            {
                throw new Exception("bad add to DataStream");
            }
            if (srcPort == iggPort)
            {
                incomingData.Add(new DataWrap(timeVal, data));
            }
            else
            {
                outgoingData.Add(new DataWrap(timeVal, data));
            }
        }
    }
}
