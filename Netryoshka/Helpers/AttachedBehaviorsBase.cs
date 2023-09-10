using System.Windows;

namespace Netryoshka
{
    public abstract class AttachedBehaviorsBase<TOwner, TSelf>
        where TOwner : FrameworkElement
        where TSelf : AttachedBehaviorsBase<TOwner, TSelf>, new()
    {
        private static TSelf? _behavior;

        public static readonly DependencyProperty AttachedBehaviorProperty =
            DependencyProperty.RegisterAttached("AttachedBehavior", typeof(bool), typeof(TOwner), new PropertyMetadata(false, OnAttachedBehaviorChanged));

        public static bool GetAttachedBehavior(DependencyObject obj)
        {
            return (bool)obj.GetValue(AttachedBehaviorProperty);
        }

        public static void SetAttachedBehavior(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachedBehaviorProperty, value);
        }

        protected abstract void OnBehaviorAttached(TOwner element);
        protected abstract void OnBehaviorDetached(TOwner element);

        private static void OnAttachedBehaviorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is TOwner element && e.NewValue is bool hasBehavior)
            {
                _behavior ??= new();

                if (hasBehavior)
                {
                    _behavior.OnBehaviorAttached(element);
                }
                else
                {
                    _behavior.OnBehaviorDetached(element);
                }
            }
        }
    }
}
