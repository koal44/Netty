namespace Netryoshka
{
    public class WireSharkData
    {
        public string JsonString { get; }
        public WireSharkPacket WireSharkPacket { get; }

        public WireSharkData(string jsonString, WireSharkPacket wireSharkPacket)
        {
            JsonString = jsonString;
            WireSharkPacket = wireSharkPacket;
        }
    }
}
