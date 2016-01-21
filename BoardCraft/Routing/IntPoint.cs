namespace BoardCraft.Routing
{
    public struct IntPoint
    {
        public bool Equals(IntPoint b)
        {
            return b.X == X && b.Y == Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is IntPoint && Equals((IntPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        public static bool operator ==(IntPoint a, IntPoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(IntPoint a, IntPoint b)
        {
            return !a.Equals(b);
        }

        public IntPoint(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly int X;
        public readonly int Y;
    }

    public struct LPoint
    {
        public bool Equals(LPoint other)
        {
            return other.Point == Point && other.Layer == Layer;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            return obj is LPoint && Equals((LPoint)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)Layer * 397) ^ Point.GetHashCode();
            }
        }

        public static bool operator ==(LPoint a, LPoint b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(LPoint a, LPoint b)
        {
            return !a.Equals(b);
        }

        public LPoint(WorkspaceLayer layer, IntPoint point)
        {
            Layer = layer;
            Point = point;
        }

        public readonly WorkspaceLayer Layer;
        public readonly IntPoint Point;
    }
}
