namespace BoardCraft.Drawing.Shapes
{
    public class Line : Shape
    {
        public Line(Point p1, Point p2)
        {
            P1 = p1;
            P2 = p2;
        }


        Point P1 { get; }
        Point P2 { get; }

        public override void DrawTo(ICanvas canvas)
        {
            canvas.DrawLine(P1, P2);
        }
    }
}
