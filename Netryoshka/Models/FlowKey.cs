namespace Netryoshka.Services
{
    public class FlowKey
    {
        public FlowEndpoint Endpoint1 { get; }
        public FlowEndpoint Endpoint2 { get; }

        public FlowKey(FlowEndpoint endpoint1, FlowEndpoint endpoint2)
        {
            Endpoint1 = endpoint1;
            Endpoint2 = endpoint2;
        }

        public override bool Equals(object? obj)
        {
            if (obj is FlowKey other)
            {
                // (1111:22,3333:44) & (5555:66,7777:88) 
                return Endpoint1.Equals(other.Endpoint1) && Endpoint2.Equals(other.Endpoint2) ||
                       Endpoint1.Equals(other.Endpoint2) && Endpoint2.Equals(other.Endpoint1);
            }
            return false;
        }

        //public override int GetHashCode() => HashCode.Combine(Endpoint1, Endpoint2);

        public override int GetHashCode()
        {
            int hash1 = Endpoint1.GetHashCode();
            int hash2 = Endpoint2.GetHashCode();

            return hash1 ^ hash2; // XOR operation ensures order doesn't matter
        }


        public override string ToString() => $"{Endpoint1.IpAddress}:{Endpoint1.Port} <--> {Endpoint2.IpAddress}:{Endpoint2.Port}";
    }





}
