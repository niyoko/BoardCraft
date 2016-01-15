namespace BoardCraft.Drawing.Shapes
{
    using System;

    public class Rectangle : Shape
    {
        public Rectangle(Point bottomLeft, double width, double height)
        {
            BottomLeft = bottomLeft;
            Width = width;
            Height = height;
        }

        public Point BottomLeft { get; }

        public double Width { get; }

        public double Height { get; }

        public override void DrawTo(ICanvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            canvas.DrawRectangle(DrawingMode.Component, BottomLeft, Width, Height);
        }
    }
}