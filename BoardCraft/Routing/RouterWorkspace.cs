namespace BoardCraft.Routing
{
    using Drawing;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NLog;
    using System.Runtime.CompilerServices;

    public enum WorkspaceLayer
    {
        BottomLayer,
        TopLayer
    }

    public sealed class BoardMargin
    {
        internal BoardMargin()
        {
            Top = 200;
            Left = 200;
            Bottom = 200;
            Right = 200;
        }

        public int Top { get; }
        public int Left { get; }
        public int Bottom { get; }
        public int Right { get; }
    }

    internal enum CellMetadata : byte
    {
        Freelane = 0,
        Obstacle = 1,
        SuspendedObstacle = 2
    }

    internal class RouterWorkspace
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();

        //data
        internal readonly int[,,] Data;
        internal readonly CellMetadata[,,] Metadata;

        //obstacle data
        internal readonly IDictionary<ComponentPin, ISet<LPoint>> PinObstacle;       
        internal readonly IDictionary<Connection, ISet<LPoint>> TraceObstacle;
        internal readonly IDictionary<Connection, ISet<LPoint>> ViaObstacle; 

        private readonly double _cellSize;

        internal readonly IDictionary<Connection, ISet<LPoint>> TracePoint;
        internal readonly IDictionary<Connection, ICollection<AbstractTraceSegment>> TraceSegments;
        internal readonly IDictionary<Connection, ISet<IntPoint>> Vias; 

        public Board Board { get; }

        public int OffsetX { get; }
        public int OffsetY { get; }

        public int Width { get; }
        public int Height { get; }        

        public RouterWorkspace(Board board, double cellSize)
        {
            Board = board;
            var s = board.GetSize();
            _cellSize = cellSize;

            OffsetX = (int) Math.Ceiling(board.Margin.Left/cellSize);
            OffsetY = (int) Math.Ceiling(board.Margin.Bottom/cellSize);

            var extrax = (int) Math.Ceiling(board.Margin.Right/cellSize);
            var extray = (int) Math.Ceiling(board.Margin.Top/cellSize);

            var bWidth = (int) Math.Ceiling(s.Width/cellSize);
            var bHeight = (int) Math.Ceiling(s.Height/cellSize);

            Width = OffsetX + bWidth + extrax;
            Height = OffsetY + bHeight + extray;

            Data = new int[2, Width, Height];
            Metadata = new CellMetadata[2, Width, Height];

            var pinCount = Board.Schema.Components.Select(x => x.Pins.Count).Sum();
            var conCount = Board.Schema.Connections.Count;

            PinObstacle = new Dictionary<ComponentPin, ISet<LPoint>>(pinCount);
            TraceObstacle = new Dictionary<Connection, ISet<LPoint>>(conCount);
            ViaObstacle = new Dictionary<Connection, ISet<LPoint>>(conCount);

            TracePoint = new Dictionary<Connection, ISet<LPoint>>(conCount);
            TraceSegments = new Dictionary<Connection, ICollection<AbstractTraceSegment>>(conCount);
            Vias = new Dictionary<Connection, ISet<IntPoint>>(conCount);
        }

        internal IntPoint PointToIntPoint(Point p)
        {
            var px = p.X/_cellSize;
            var py = p.Y/_cellSize;

            px = Math.Round(px);
            py = Math.Round(py);

            return new IntPoint((int)px + OffsetX, (int)py + OffsetY);
        }

        TraceSegment TranslateSegment(AbstractTraceSegment segment)
        {
            var l = segment.Layer == WorkspaceLayer.BottomLayer ? TraceLayer.BottomLayer : TraceLayer.TopLayer;
            var s = new TraceSegment(l);

            var nds = segment.Nodes.Select(IntPointToPoint);
            foreach (var p in nds)
            {
                s.AddNode(p);
            }

            return s;
        }

        internal void WriteToBoard()
        {
            var s = new Dictionary<Connection, ICollection<TraceSegment>>(TraceSegments.Count);
            foreach (var segment in TraceSegments)
            {
                var val = new List<TraceSegment>(segment.Value.Select(TranslateSegment));
                s.Add(segment.Key, val);
            }

            Board.TraceSegments = s;
            
            var vias =  new Dictionary<Connection, ISet<Point>>();
            foreach (var via in Vias)
            {
                var v = via.Value.Select(IntPointToPoint);
                vias.Add(via.Key, new HashSet<Point>(v));
            }

            Board.Vias = vias;
        }

        internal Point IntPointToPoint(IntPoint p)
        {
            var px = (.5 + p.X - OffsetX) * _cellSize;
            var py = (.5 + p.Y - OffsetY) * _cellSize;

            return new Point(px, py);
        }

        public void SetupWorkspaceForRouting(Connection connection)
        {
            Array.Clear(Data, 0, Data.Length);
            Array.Clear(Metadata, 0, Metadata.Length);

            var tupled = connection.Pins.Select(x => x.Pin);
            var hashTupled = new HashSet<ComponentPin>(tupled);
            
            foreach (var o in PinObstacle)
            {
                var isInsideCurrentConection = hashTupled.Contains(o.Key);
                foreach (var ic in o.Value)
                {
                    var isSuspended = isInsideCurrentConection 
                        && ic.Layer == WorkspaceLayer.BottomLayer;

                    SetMetadata(ic, isSuspended ? 
                        CellMetadata.SuspendedObstacle 
                        : CellMetadata.Obstacle);
                }
            }

            foreach (var o in TraceObstacle)
            {
                foreach (var v in o.Value)
                {
                    SetMetadata(v, CellMetadata.Obstacle);
                }
            }

            foreach (var v in ViaObstacle)
            {
                foreach (var v2 in v.Value)
                {                    
                    SetMetadata(v2, CellMetadata.Obstacle);                    
                }
            }
        }

        public bool IsPointValid(IntPoint index)
        {
            var valid = index.X >= 0;
            valid = valid && index.X < Width;
            valid = valid && index.Y >= 0;
            valid = valid && index.Y < Height;
            return valid;
        }

        public int this[LPoint index]
        {
            get { return Data[(int)index.Layer, index.Point.X, index.Point.Y]; }
            set { Data[(int)index.Layer, index.Point.X, index.Point.Y] = value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public CellMetadata GetMetadata(LPoint index)
        {
            return Metadata[(int) index.Layer, index.Point.X, index.Point.Y];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetMetadata(LPoint index, CellMetadata value)
        {
            Metadata[(int) index.Layer, index.Point.X, index.Point.Y] = value;
        }
    }
}