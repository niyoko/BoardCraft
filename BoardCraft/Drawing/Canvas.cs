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

        public abstract void DrawRectangle(Point bottomLeft, double width, double height);

        public abstract void DrawEllipse(Point center, double xRadius, double yRadius);

        public abstract void DrawLine(Point point1, Point point2);
    }
}