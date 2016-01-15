using System;

namespace BoardCraft.Drawing.PinStyles
{
    public class DILPinStyle : PinStyle
    {
        public double PadWidth { get; }
        public double DrillDiameter { get; }

        public DILPinStyle(double padWidth, double drillDiameter)
        {
            PadWidth = padWidth;
            DrillDiameter = drillDiameter;
        }

        public override void DrawTo(ICanvas canvas, Point position)
        {
            var posx = position.X - 0.25 * PadWidth;
            var posy = position.Y - 0.5 * PadWidth;
            var pos = new Point(posx, posy);

            canvas.DrawFilledRectangle(DrawingMode.Pad, pos, 0.5 * PadWidth, PadWidth);
            canvas.DrawFilledEllipse(DrawingMode.DrillHole, position, 0.5 * DrillDiameter, 0.5 * DrillDiameter);
        }
    }
}
