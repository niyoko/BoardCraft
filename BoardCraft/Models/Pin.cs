namespace BoardCraft.Models
{
    using System.Globalization;
    using Drawing;
    using Drawing.PinStyles;

    public class Pin
    {
        public Pin(string name, Point position, PinStyle style)
        {
            Name = name;
            Position = position;
            Style = style;
        }

        public Component Parent { get; }
        public string Name { get; }

        public Point Position { get; }
        public PinStyle Style { get; }

        public void DrawPad(ICanvas canvas)
        {
            Style.DrawTo(canvas, Position);
        }

        public void DrawDrillHole(ICanvas canvas)
        {
            Style.DrawDrillHole(canvas, Position);
        }
    }

    public class ComponentPin
    {
        public ComponentPin(Component component, Pin packagePin)
        {
            Component = component;
            PackagePin = packagePin;
        }

        public Component Component { get; }
        public Pin PackagePin { get; }
        public string Name => PackagePin.Name;
    }
}