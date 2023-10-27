using System;

namespace Netryoshka.ViewModels.ChatBubbles
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CanContentScrollAttribute : Attribute
    {
        public bool CanScroll { get; }

        public CanContentScrollAttribute(bool canScroll)
        {
            CanScroll = canScroll;
        }
    }
}
