using System.Windows;
using System.Windows.Controls;

namespace BoardCraft.Output.Wpf
{
    using System.Diagnostics;
    using Drawing;
    using Models;

    /// <summary>
    /// Interaction logic for BoardCanvas.xaml
    /// </summary>
    public partial class BoardCanvas : UserControl
    {
        public BoardCanvas()
        {
            InitializeComponent();
            Canvas = new WpfCanvas(NativeCanvas);
        }

        public ICanvas Canvas { get; }

        public ComponentPlacement ComponentPlacement
        {
            get { return GetValue(ComponentPlacementProperty) as ComponentPlacement; }
            set { SetValue(ComponentPlacementProperty, value); }
        }

        static void Cb(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            var that = (BoardCanvas)o;
            var plc = (Models.ComponentPlacement)args.NewValue;
            if (plc == null)
            {
                that.Canvas.Clear();
            }
            else
            {
                plc.Draw(that.Canvas);
            }
                     
        }

        public static readonly DependencyProperty ComponentPlacementProperty =
            DependencyProperty.Register(nameof(ComponentPlacement), 
                typeof(ComponentPlacement), 
                typeof(BoardCanvas), 
                new FrameworkPropertyMetadata(Cb));
    }
}
