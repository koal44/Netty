using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace Netryoshka.ViewModels.ChatBubbles
{
    public abstract partial class BubbleViewModelBase : ObservableObject
    {
        private static Dictionary<string, Type>? _registeredTypes;
        public static Dictionary<string, Type> RegisteredTypes
        {
            get
            {
                if (_registeredTypes == null)
                {
                    _registeredTypes = new Dictionary<string, Type>();
                    foreach (var type in typeof(BubbleViewModelBase).Assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(BubbleViewModelBase)))
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
        private string? _headerContent;
        [ObservableProperty]
        private string? _bodyContent;
        [ObservableProperty]
        private string? _footerContent;


        protected BubbleViewModelBase()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                EndPointRole = FlowEndpointRole.Pivot;
                HeaderContent = "Header";
                BodyContent = "Body";
                FooterContent = TimeSpan.Zero.ToString("mm\\.ss\\.ffff");
            }
        }


        protected BubbleViewModelBase(BubbleData data)
        {
            EndPointRole = data.EndPointRole;
            FooterContent = $"#{data.BubbleIndex} {data.PacketInterval:mm\\.ss\\.ffff}";
        }
    }
}
