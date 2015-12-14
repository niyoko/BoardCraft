namespace BoardCraft.Drawing
{
    /// <summary>
    ///     Represent a point in two-dimensional cartesian coordinate system
    /// </summary>
    public struct Point
    {
        /// <summary>
        ///     (0,0) point.
        /// </summary>
        public static readonly Point Origin = new Point(0, 0);

        /// <summary>
        ///     Create a <see cref="Point" /> from x and y part
        /// </summary>
        /// <param name="x">X part of <see cref="Point" /></param>
        /// <param name="y">Y part of <see cref="Point" /></param>
        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        ///     X part of this <see cref="Point" />
        /// </summary>
        public double X { get; }

        /// <summary>
        ///     Y part of this <see cref="Point" />
        /// </summary>
        public double Y { get; }

        /// <summary>
        ///     Override <see cref="System.Object.ToString()" /> method
        /// </summary>
        /// <returns>String representation of this <see cref="Point" /></returns>
        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}