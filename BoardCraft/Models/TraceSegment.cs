namespace BoardCraft.Models
{
    using System.Collections.Generic;
    using Drawing;

    internal sealed class TraceSegment
    {
        public IList<Point> Nodes { get; }
        public TraceLayer Layer { get; }

        public TraceSegment(TraceLayer layer)
        {
            Nodes = new List<Point>();
            Layer = layer;
        }

        public void AddNode(Point point)
        {
            Nodes.Add(point);
        }

        public void Draw(ICanvas canvas)
        {
            
        }
    }
}
