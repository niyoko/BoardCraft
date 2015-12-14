namespace BoardCraft.Drawing
{
    public interface ICanvas
    {
        Transform Transform { get; }

        void Clear();

        void DrawRectangle(Point bottomLeft, double width, double height);

        void DrawEllipse(Point center, double xRadius, double yRadius);

        void DrawLine(Point a, Point b);
    }
}