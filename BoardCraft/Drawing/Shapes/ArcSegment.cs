using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Drawing.Shapes
{
    public class ArcSegment : Shape
    {
        public Point Center { get; }
        public double XRadius { get; }
        public double YRadius { get; }
        public double StartAngle { get; }
        public double EndAngle { get; }

        public ArcSegment(Point center, double xRadius, double yRadius, double startAngle, double endAngle)
        {
            Center = center;
            XRadius = xRadius;
            YRadius = yRadius;
            StartAngle = startAngle;
            EndAngle = endAngle;
        }

        public override void DrawTo(ICanvas canvas)
        {
            canvas.DrawArcSegment(DrawingMode.Component, Center, XRadius, YRadius, StartAngle, EndAngle);
        }
    }
}
