namespace BoardCraft.Drawing.Shapes
{
    using System;

    public class Ellipse : Shape
    {
        public Ellipse(Point center, double xRadius, double yRadius)
        {
            Center = center;
            XRadius = xRadius;
            YRadius = yRadius;
        }

        public Point Center { get; }

        public double XRadius { get; }

        public double YRadius { get; }

        public override void DrawTo(ICanvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            canvas.DrawEllipse(DrawingMode.Component, Center, XRadius, YRadius);
        }
    }
}