using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Netryoshka
{
    public partial class BotButton : UserControl
    {
        public BotButton()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty IsSelectedProperty =
        DependencyProperty.Register("IsSelected", typeof(bool), typeof(BotButton), new PropertyMetadata(false));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public static readonly DependencyProperty FlowEndpointRoleProperty =
            DependencyProperty.Register("FlowEndpointRole", typeof(FlowEndpointRole), typeof(BotButton), new PropertyMetadata(FlowEndpointRole.Pivot));

        public FlowEndpointRole FlowEndpointRole
        {
            get { return (FlowEndpointRole)GetValue(FlowEndpointRoleProperty); }
            set { SetValue(FlowEndpointRoleProperty, value); }
        }

        public static readonly DependencyProperty PathDataProperty =
        DependencyProperty.Register("PathData", typeof(Geometry), typeof(BotButton), new PropertyMetadata(null));

        public Geometry PathData
        {
            get { return (Geometry)GetValue(PathDataProperty); }
            set { SetValue(PathDataProperty, value); }
        }


        public static readonly DependencyProperty BotPaddingProperty =
    DependencyProperty.Register("BotPadding", typeof(Thickness), typeof(BotButton), new PropertyMetadata(new Thickness(4, 1, 4, 4)));

        public Thickness BotPadding
        {
            get => (Thickness)GetValue(BotPaddingProperty);
            set => SetValue(BotPaddingProperty, value);
        }
    }
}
