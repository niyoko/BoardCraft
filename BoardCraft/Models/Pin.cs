namespace BoardCraft.Models
{
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

        public string Name { get; set; }

        public Point Position { get; set; }
        public PinStyle Style { get; set; }

        public void DrawPad(ICanvas canvas)
        {
            Style.DrawTo(canvas, Position);
        }

        public void DrawDrillHole(ICanvas canvas)
        {
            Style.DrawDrillHole(canvas, Position);
        }
    }
}