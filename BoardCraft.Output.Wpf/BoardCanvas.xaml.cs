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

        public WpfCanvas Canvas { get; }

        public Board ComponentPlacement
        {
            get { return GetValue(ComponentPlacementProperty) as Board; }
            set { SetValue(ComponentPlacementProperty, value); }
        }

        public bool Component
        {
            get { return (bool)GetValue(ComponentProperty); }
            set { SetValue(ComponentProperty, value); }
        }

        public bool BottomCopper
        {
            get { return (bool)GetValue(BottomCopperProperty); }
            set { SetValue(BottomCopperProperty, value); }
        }

        public bool TopCopper
        {
            get { return (bool)GetValue(TopCopperProperty); }
            set { SetValue(TopCopperProperty, value); }
        }

        public bool Pad
        {
            get { return (bool)GetValue(PadProperty); }
            set { SetValue(PadProperty, value); }
        }

        public bool DrillHole
        {
            get { return (bool)GetValue(DrillHoleProperty); }
            set { SetValue(DrillHoleProperty, value); }
        }

        public bool Via
        {
            get { return (bool)GetValue(ViaProperty); }
            set { SetValue(ViaProperty, value); }
        }

        public bool BoardEdge
        {
            get { return (bool)GetValue(BoardEdgeProperty); }
            set { SetValue(BoardEdgeProperty, value); }
        }

        public ColorPallete ColorPallete
        {
            get { return (ColorPallete)GetValue(ColorPalleteProperty); }
            set { SetValue(ColorPalleteProperty, value); }
        }

        static void Cb(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            Debug.WriteLine($"Property changed {args.Property.Name}");
            var that = (BoardCanvas)o;

            that.Canvas.Component = that.Component;
            that.Canvas.BottomCopper = that.BottomCopper;
            that.Canvas.TopCopper = that.TopCopper;
            that.Canvas.DrillHole = that.DrillHole;
            that.Canvas.Pad = that.Pad;
            that.Canvas.Via = that.Via;
            that.Canvas.BoardEdge = that.BoardEdge;
            that.Canvas.ColorPallete = that.ColorPallete;

            var plc = that.ComponentPlacement;
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
            DependencyProperty.Register(
                nameof(ComponentPlacement),
                typeof(Board),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty ComponentProperty =
            DependencyProperty.Register(
                nameof(Component),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty BottomCopperProperty =
            DependencyProperty.Register(
                nameof(BottomCopper),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty TopCopperProperty =
            DependencyProperty.Register(
                nameof(TopCopper),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty PadProperty =
            DependencyProperty.Register(
                nameof(Pad),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty DrillHoleProperty =
            DependencyProperty.Register(
                nameof(DrillHole),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty ViaProperty =
            DependencyProperty.Register(
                nameof(Via),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty BoardEdgeProperty =
            DependencyProperty.Register(
                nameof(BoardEdge),
                typeof(bool),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));

        public static readonly DependencyProperty ColorPalleteProperty = 
            DependencyProperty.Register(
                nameof(ColorPallete),
                typeof(ColorPallete),
                typeof(BoardCanvas),
                new FrameworkPropertyMetadata(Cb));
    }
}
