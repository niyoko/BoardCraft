namespace BoardCraft.Drawing.Shapes
{
    public class Circle : Ellipse
    {
        public Circle(Point center, double radius) : base(center, radius, radius)
        {
        }

        public double Radius => XRadius;
    }
}