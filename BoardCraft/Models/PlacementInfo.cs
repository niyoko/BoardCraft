namespace BoardCraft.Models
{
    using Drawing;

    public struct PlacementInfo
    {
        public PlacementInfo(Point position, Orientation orientation)
        {
            Position = position;
            Orientation = orientation;
        }

        public readonly Point Position;
        public readonly Orientation Orientation;
    }
}
