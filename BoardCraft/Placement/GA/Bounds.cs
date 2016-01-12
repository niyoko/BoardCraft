namespace BoardCraft.Placement.GA
{
    public struct Bounds
    {
        public Bounds(double top, double right, double bottom, double left)
        {
            Top = top;
            Right = right;
            Bottom = bottom;
            Left = left;
        }

        public readonly double Top;
        public readonly double Right;
        public readonly double Bottom;
        public readonly double Left;
    }
}
