namespace BoardCraft.Routing
{
    using Drawing;
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using NLog;
    using System.IO;
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

        internal readonly int[,,] Data;
        internal readonly CellMetadata[,,] Metadata;

        internal readonly IDictionary<Tuple<Component, string>, ISet<LPoint>> PinObstacle;
        internal readonly IDictionary<Connection, ISet<LPoint>> TrackObstacle;

        private readonly double _cellSize;

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

            PinObstacle = new Dictionary<Tuple<Component, string>, ISet<LPoint>>(Board.Schema.Connections.Count);
            TrackObstacle = new Dictionary<Connection, ISet<LPoint>>();
        }

        internal IntPoint PointToIntPoint(Point p)
        {
            var px = p.X/_cellSize;
            var py = p.Y/_cellSize;

            px = Math.Round(px);
            py = Math.Round(py);

            return new IntPoint((int)px + OffsetX, (int)py + OffsetY);
        }

        internal Point IntPointToPoint(IntPoint p)
        {
            var px = (.5 + p.X - OffsetX) * _cellSize;
            var py = (.5 + p.Y - OffsetY) * _cellSize;

            return new Point(px, py);
        }

        public void SetPinObstacle(Component component, string pinName, IEnumerable<LPoint> obstacle)
        {
            var key = Tuple.Create(component, pinName);
            var el = new HashSet<LPoint>(obstacle);
            PinObstacle.Add(key, el);
        }

        public void SetTrackObstacle(Connection connection, ISet<LPoint> obstacle)
        {
            var el = obstacle;
            TrackObstacle.Add(connection, el);
        }

        public void SetupWorkspaceForRouting(Connection connection)
        {
            Array.Clear(Data, 0, Data.Length);
            Array.Clear(Metadata, 0, Metadata.Length);

            var tupled = connection.Pins.Select(x => Tuple.Create(x.Component, x.Pin));
            var hashTupled = new HashSet<Tuple<Component, string>>(tupled);
            
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

            foreach (var o in TrackObstacle)
            {
                foreach (var v in o.Value)
                {
                    SetMetadata(v, CellMetadata.Obstacle);
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

#if DEBUG
        internal void DumpState()
        {
            var s2 = new StringBuilder();
            for (var k = 0; k < 2; k++)
            {
                for (var j = Height - 1; j >= 0; j--)
                {
                    for (var i = 0; i < Width; i++)
                    {
                            
                        var v = new LPoint((WorkspaceLayer)k, new IntPoint(i, j));
                        var s = this[v].ToString();
                        s = s.PadLeft(5, ' ');
                        s2.Append(s);
                        s2.Append(",");
                    }

                    s2.AppendLine();
                }

                s2.Append("----------------------------");
                s2.AppendLine();
            }

            File.WriteAllText(@"D:\debug.txt", s2.ToString());
        }

        internal void Render(ICanvas canvas)
        {
            Debug.WriteLine("Workspace rendered");
            for (var k = 0; k < 2; k++)
            {
                for (var j = Height - 1; j >= 0; j--)
                {
                    for (var i = 0; i < Width; i++)
                    {
                        var idx = new LPoint((WorkspaceLayer)k, new IntPoint(i, j));
                        if (idx.Layer == WorkspaceLayer.BottomLayer && this[idx] == -1)
                        {
                            canvas.DrawRectangle(DrawingMode.DrillHole, IntPointToPoint(idx.Point), _cellSize, _cellSize);
                        }
                    }
                }
            }            
        }
#endif
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