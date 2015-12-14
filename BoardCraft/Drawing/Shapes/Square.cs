namespace BoardCraft.Drawing.Shapes
{
    public class Square : Rectangle
    {
        public Square(Point bottomLeft, double side) : base(bottomLeft, side, side)
        {
        }

        public double Side => Width;
    }
}