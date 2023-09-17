using CommunityToolkit.Mvvm.ComponentModel;

namespace Netryoshka
{
    public partial class WireSharkData : ObservableObject
    {
        [ObservableProperty]
        private string _jsonString;
        [ObservableProperty]
        private WireSharkPacket _wireSharkPacket;

        public WireSharkData(string jsonString, WireSharkPacket wireSharkPacket)
        {
            _jsonString = jsonString;
            _wireSharkPacket = wireSharkPacket;
        }
    }
}
