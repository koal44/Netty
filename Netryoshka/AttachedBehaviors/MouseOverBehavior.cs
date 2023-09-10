using System;
using System.Windows;

namespace Netryoshka
{
    /* WARNING:
        This only works if a view's state is determined by one if its elements  
        state. totally worthless and there are easier ways to do this. GOODBYE.
        This class is deprecated and should NOT be used.
        Left for historical or instructional value. */

    [Obsolete("This class is deprecated and should not be used.")]
    public class MouseOverBehavior 
        : AttachedBehaviorsBase<FrameworkElement, MouseOverBehavior>
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

        /// <summary>
        /// Handles the MouseEnter event of the associated FrameworkElement.
        /// </summary>
        /// <param name="sender">The source of the event, expected to be a FrameworkElement.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// This method sets the IsMouseOver property to true on the FrameworkElement's DataContext, 
        /// provided it implements the <see cref="IViewModelWithMouseOverBehavior"> interface.
        /// </remarks>
        private void OnMouseEnter(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is IViewModelWithMouseOverBehavior flowChatBubble)
            {
                flowChatBubble.IsMouseOver = true;
            }
        }

        private void OnMouseLeave(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is IViewModelWithMouseOverBehavior flowChatBubble)
            {
                flowChatBubble.IsMouseOver = false;
            }
        }

    }
}
