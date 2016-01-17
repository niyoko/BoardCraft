namespace BoardCraft.Routing
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public enum WorkspaceLayer
    {
        BottomLayer,
        TopLayer
    }


    internal class RouterWorkspace
    {
        internal readonly int[,,] _data;
        internal readonly IDictionary<Tuple<Component, string>, ISet<LPoint>> _pinObstacle;
        internal readonly IDictionary<Connection, ISet<LPoint>> _trackObstacle;

        private readonly double _cellSize;

        public Board Board { get; }

        public int Width { get; }
        public int Height { get; }

        public RouterWorkspace(Board board, double cellSize)
        {
            Board = board;
            var s = board.GetSize();
            _cellSize = cellSize;

            Width = (int)Math.Ceiling((s.Width+500)/cellSize);
            Height = (int)Math.Ceiling((s.Height+500)/cellSize);

            _data = new int[2, Width, Height];
            _pinObstacle = new Dictionary<Tuple<Component, string>, ISet<LPoint>>(Board.Schema.Connections.Count);
            _trackObstacle = new Dictionary<Connection, ISet<LPoint>>();
        }

        public void SetPinObstacle(Component component, string pinName, IEnumerable<LPoint> obstacle)
        {
            var key = Tuple.Create(component, pinName);
            var el = new HashSet<LPoint>(obstacle);
            _pinObstacle.Add(key, el);
        }

        public void SetTrackObstacle(Connection connection, IEnumerable<LPoint> obstacle)
        {
            var el = new HashSet<LPoint>(obstacle);
            _trackObstacle.Add(connection, el);
        }

        public void SetupWorkspaceForRouting(Connection connection)
        {
            Array.Clear(_data, 0, _data.Length);

            var tupled = connection.Pins.Select(x => Tuple.Create(x.Component, x.Pin));
            var hashTupled = new HashSet<Tuple<Component, string>>(tupled);

            foreach (var o in _pinObstacle)
            {
                var isInsideCurrentConection = hashTupled.Contains(o.Key);
                foreach (var ic in o.Value)
                {
                    if (!isInsideCurrentConection || ic.Layer == WorkspaceLayer.TopLayer)
                    {
                        this[ic] = -1;
                    }
                }
            }

            foreach (var o in _trackObstacle)
            {
                foreach (var v in o.Value)
                {
                    this[v] = -1;
                }
            }
        }

        public bool IsPointValid(IntPoint index)
        {
            var valid = true;
            valid = valid && index.X >= 0;
            valid = valid && index.X < Width;
            valid = valid && index.Y >= 0;
            valid = valid && index.Y < Height;
            return valid;
        }

        public int this[LPoint index]
        {
            get { return _data[(int)index.Layer, index.Point.X, index.Point.Y]; }
            set { _data[(int)index.Layer, index.Point.X, index.Point.Y] = value; }
        }
    }
}