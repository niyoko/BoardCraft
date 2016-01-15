namespace BoardCraft.Drawing
{
    public interface ICanvas
    {
        Transform Transform { get; }

        void Clear();

        void DrawRectangle(DrawingMode mode, Point bottomLeft, double width, double height);

        void DrawFilledRectangle(DrawingMode mode, Point bottomLeft, double width, double height);

        void DrawEllipse(DrawingMode mode, Point center, double xRadius, double yRadius);

        void DrawFilledEllipse(DrawingMode mode, Point center, double xRadius, double yRadius);

        void DrawFilledDonut(DrawingMode mode, Point center, double xRadius, double yRadius);

        void DrawLine(DrawingMode mode, Point a, Point b);
    }
}