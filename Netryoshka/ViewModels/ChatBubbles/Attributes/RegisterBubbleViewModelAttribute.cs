using System;

namespace Netryoshka.ViewModels.ChatBubbles
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterBubbleViewModelAttribute : Attribute
    {
        public string Key { get; set; }

        public RegisterBubbleViewModelAttribute(string key)
        {
            Key = key;
        }
    }
}