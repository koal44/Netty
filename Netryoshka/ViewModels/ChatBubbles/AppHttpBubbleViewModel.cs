using Netryoshka.Utils;
using Netryoshka.ViewModels.ChatBubbles;
using System;

namespace Netryoshka.ViewModels
{
    [CanContentScroll(true)]
    [RequiresWireShark]
    [RegisterBubbleViewModel("AppHttp")]
    public partial class AppHttpBubbleViewModel : BubbleViewModelBase
    {
        public AppHttpBubbleViewModel() : base()
        { }

        public AppHttpBubbleViewModel(BubbleData data)
            : base(data)
        {
            var httpJsonList = JsonUtils.ExtractJsonObjectsFromKey(data.WireSharkData!.JsonString!, "http");
            BodyContent = StringUtils.StringJoin(Environment.NewLine, httpJsonList);
        }
    }
}
