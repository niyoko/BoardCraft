namespace BoardCraft.Drawing.PinStyles
{
    public class CirclePinStyle : PinStyle
    {
        public double PadDiameter { get; }
        public double DrillDiameter { get; }

        public CirclePinStyle(double padDiameter, double drillDiameter)
        {
            PadDiameter = padDiameter;
            DrillDiameter = drillDiameter;
        }

        public override void DrawTo(ICanvas canvas, Point position)
        {
            canvas.DrawFilledEllipse(DrawingMode.Pad, position, 0.5*PadDiameter, 0.5*PadDiameter);            
        }

        public override void DrawDrillHole(ICanvas canvas, Point position)
        {
            canvas.DrawFilledEllipse(DrawingMode.DrillHole, position, 0.5 * DrillDiameter, 0.5 * DrillDiameter);
        }
    }
}
