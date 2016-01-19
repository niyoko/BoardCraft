namespace BoardCraft.Drawing.PinStyles
{
    public class SquarePinStyle : PinStyle
    {
        public double SquareSide { get; }
        public double DrillDiameter { get; }

        public SquarePinStyle(double squareSide, double drillDiameter)
        {
            SquareSide = squareSide;
            DrillDiameter = drillDiameter;
        }

        public override void DrawTo(ICanvas canvas, Point position)
        {
            var posx = position.X - 0.5 * SquareSide;
            var posy = position.Y - 0.5 * SquareSide;
            var pos = new Point(posx, posy);

            canvas.DrawFilledRectangle(DrawingMode.Pad, pos, SquareSide, SquareSide);            
        }

        public override void DrawDrillHole(ICanvas canvas, Point position)
        {
            canvas.DrawFilledEllipse(DrawingMode.DrillHole, position, 0.5 * DrillDiameter, 0.5 * DrillDiameter);
        }
    }
}
