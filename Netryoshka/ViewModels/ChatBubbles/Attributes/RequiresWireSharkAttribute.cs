using System;

namespace Netryoshka.ViewModels.ChatBubbles
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class RequiresWireSharkAttribute : Attribute
    {

    }
}
