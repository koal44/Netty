using System;

namespace Proton.Retire
{
    public struct DataWrap
    {
        public readonly DateTime time;
        public readonly byte[] data;

        public DataWrap(DateTime time, byte[] data)
        {
            this.time = time;
            this.data = data;
        }
    }
}
