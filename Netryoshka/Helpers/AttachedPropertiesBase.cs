using System.Windows;

namespace Proton
{
    public abstract class AttachedPropertiesBase<TOwner, TPropertyType>
    where TOwner : DependencyObject, new()
    {
        public static readonly DependencyProperty AttachedProperty =
            DependencyProperty.RegisterAttached("Value", typeof(TPropertyType), typeof(TOwner), new PropertyMetadata(default(TPropertyType)));

        public static TPropertyType GetValue(DependencyObject obj)
        {
            return (TPropertyType)obj.GetValue(AttachedProperty);
        }

        public static void SetValue(DependencyObject obj, TPropertyType value)
        {
            obj.SetValue(AttachedProperty, value);
        }
    }
}
