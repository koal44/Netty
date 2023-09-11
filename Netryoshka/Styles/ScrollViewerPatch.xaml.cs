using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Netryoshka
{
    public partial class ScrollViewerPatch : ResourceDictionary
    {
        public ScrollViewerPatch()
        {

        }

        public void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            if (scrollViewer.ComputedVerticalScrollBarVisibility == Visibility.Collapsed)
            {
                e.Handled = false;
                return;
            }
            // Call base implementation here. This requires casting sender to the base type.
            //(sender as ScrollViewer)?.OnMouseWheel(e);

            // Call protected method here. This requires reflection.
            var method = typeof(ScrollViewer).GetMethod("OnMouseWheel", BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(scrollViewer, new object[] { e });
        }
    }

}
