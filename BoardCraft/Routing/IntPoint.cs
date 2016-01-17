namespace BoardCraft.Routing
{
    public struct IntPoint
    {
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
        public LPoint(WorkspaceLayer layer, IntPoint point)
        {
            Layer = layer;
            Point = point;
        }

        public readonly WorkspaceLayer Layer;
        public readonly IntPoint Point;
    }
}
