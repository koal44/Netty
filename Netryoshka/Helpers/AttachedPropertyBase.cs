using System;
using System.Windows;

namespace Netty
{
    public abstract class AttachedPropertyBase<TOwner, TValue>
    where TOwner : AttachedPropertyBase<TOwner, TValue>, new()
    {
        /*https://github.com/angelsix/fasetto-word/blob/7c1698c39c78349287afd84da714bf1acc42633d/Source/Fasetto.Word/AttachedProperties/BaseAttachedProperty.cs*/


        // Thread-safe lazy initialization for the singleton instance
        private static readonly Lazy<TOwner> _instance = new(() => new TOwner());
        public static TOwner Instance => _instance.Value;

        /// <summary>
        /// Event fired when the Value property changes.
        /// </summary>
        public event Action<DependencyObject, DependencyPropertyChangedEventArgs> ValueChanged = (sender, e) => { };

        /// <summary>
        /// Attached DependencyProperty for storing the value.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.RegisterAttached("Value", typeof(TValue), typeof(AttachedPropertyBase<TOwner, TValue>), new PropertyMetadata(OnValuePropertyChanged));

        /// <summary>
        /// Callback when the Value property changes.
        /// </summary>
        private static void OnValuePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Invoke virtual method and event
            Instance.OnValueChanged(d, e);
            Instance.InvokeValueChangedEvent(d, e);
        }

        /// <summary>
        /// Virtual method invoked when the Value property changes.
        /// </summary>
        public virtual void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) { }

        /// <summary>
        /// Safely invoke the ValueChanged.
        /// </summary>
        protected void InvokeValueChangedEvent(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(d, e);
        }

        /// <summary>
        /// Gets the value of the attached property.
        /// </summary>
        public static TValue GetValue(DependencyObject d) => (TValue)d.GetValue(ValueProperty);

        /// <summary>
        /// Sets the value of the attached property.
        /// </summary>
        public static void SetValue(DependencyObject d, TValue value) => d.SetValue(ValueProperty, value);
    }
}
