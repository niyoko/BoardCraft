namespace BoardCraft.Drawing
{
    public abstract class Canvas : ICanvas
    {
        protected Canvas()
        {
            Transform = new Transform();
        }

        public Transform Transform { get; }

        public abstract void Clear();

        public abstract void DrawRectangle(DrawingMode mode, Point bottomLeft, double width, double height);
        public abstract void DrawFilledRectangle(DrawingMode mode, Point bottomLeft, double width, double height);
        public abstract void DrawEllipse(DrawingMode mode, Point center, double xRadius, double yRadius);
        public abstract void DrawFilledEllipse(DrawingMode mode, Point center, double xRadius, double yRadius);        
        public abstract void DrawLine(DrawingMode mode, Point a, Point b);
        public abstract void DrawArcSegment(DrawingMode mode, Point center, double xRadius, double yRadius, double startAngle, double endAngle);        
    }
}