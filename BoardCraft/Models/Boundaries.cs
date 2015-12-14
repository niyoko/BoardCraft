namespace BoardCraft.Models
{
    using System;

    public struct Boundaries
    {
        public Boundaries(double top, double right, double bottom, double left)
        {
            if (top < bottom)
            {
                throw new ArgumentException("Top cannot less than bottom", nameof(top));
            }

            if (right < left)
            {
                throw new ArgumentException("Right cannot less than left", nameof(right));
            }

            Top = top;
            Right = right;            
            Bottom = bottom;
            Left = left;
        }

        public double Top { get; }

        public double Left { get; }

        public double Right { get; }

        public double Bottom { get; }

        public double Width => Right - Left;

        public double Height => Top - Bottom;

        public double Area => Width * Height;
    }
}