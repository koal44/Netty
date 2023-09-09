using System.Windows;

namespace Proton
{
    public class MatryoshkaButtonMouseOverBehavior 
        : AttachedBehaviorsBase<FrameworkElement, MatryoshkaButtonMouseOverBehavior>
    {
        protected override void OnBehaviorAttached(FrameworkElement element)
        {
            element.MouseEnter += OnMouseEnter;
            element.MouseLeave += OnMouseLeave;
        }

        protected override void OnBehaviorDetached(FrameworkElement element)
        {
            element.MouseEnter -= OnMouseEnter;
            element.MouseLeave -= OnMouseLeave;
        }

        private void OnMouseEnter(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is FlowChatBubbleViewModel flowChatBubble)
            {
                flowChatBubble.IsMousingOver = true;
            }
        }

        private void OnMouseLeave(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is FlowChatBubbleViewModel flowChatBubble)
            {
                flowChatBubble.IsMousingOver = false;
            }
        }

    }
}
