using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Netryoshka.ViewModels
{
    public abstract partial class BubbleViewModel : ObservableObject
    {
        private static Dictionary<string, Type>? _registeredTypes;
        public static Dictionary<string, Type> RegisteredTypes
        {
            get
            {
                if (_registeredTypes == null)
                {
                    _registeredTypes = new Dictionary<string, Type>();
                    foreach (var type in typeof(BubbleViewModel).Assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(BubbleViewModel)))
                        {
                            var attribute = type.GetCustomAttribute<RegisterBubbleViewModelAttribute>();
                            if (attribute != null)
                            {
                                _registeredTypes.Add(attribute.Key, type);
                            }
                        }
                    }
                }
                return _registeredTypes;
            }
        }


        [ObservableProperty]
        private double _bubbleScale = 0.8;
        [ObservableProperty]
        private FlowEndpointRole _endPointRole;
        [ObservableProperty]
        private string? _footerContent;


        protected BubbleViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                EndPointRole = FlowEndpointRole.Pivot;
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }


        protected BubbleViewModel(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CanContentScrollAttribute : Attribute
    {
        public bool CanScroll { get; }

        public CanContentScrollAttribute(bool canScroll)
        {
            CanScroll = canScroll;
        }
    }


    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class RegisterBubbleViewModelAttribute : Attribute
    {
        public string Key { get; set; }

        public RegisterBubbleViewModelAttribute(string key)
        {
            Key = key;
        }
    }


    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class RequiresWireSharkAttribute : Attribute
    {

    }
}
