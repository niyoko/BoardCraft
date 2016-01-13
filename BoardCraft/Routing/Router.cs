namespace BoardCraft.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using NLog;
    using Drawing;

    public class Router
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
        private double TraceWidth { get; }
        private double MinimumSpacing { get; }
        private static double CellSize = 0.2;

        public Router(double traceWidth, double minimumSpacing)
        {
            TraceWidth = traceWidth;
            MinimumSpacing = minimumSpacing;
        }

        private IntPoint PointToIntPoint(Point p)
        {
            var px = p.X/CellSize;
            var py = p.Y/CellSize;

            px = Math.Round(px);
            py = Math.Round(py);

            return new IntPoint((int) px, (int) py);
        }

        private Point IntPointToPoint(IntPoint p)
        {
            var px = (.5+p.X)*CellSize;
            var py = (.5+p.Y)*CellSize;

            return new Point(px, py);
        }

        private Dictionary<Tuple<Component, string>, ICollection<IntPoint>> _pinObs;

        private void SetupWorkspace(RouterWorkspace workspace)
        {
            var board = workspace.Board;
            const double pinRad = 1.5;
            var span = (int)Math.Round(pinRad/CellSize + 1);

            _pinObs = new Dictionary<Tuple<Component, string>, ICollection<IntPoint>>();

            var z1 = new List<int>();
            for (var i = 0; i < span; i++)
            {
                var th = Math.Acos((double)i/span);
                var leftSpan = (int)(Math.Round(Math.Sin(th)*span));
                z1.Add(leftSpan);
            }
            
            board.CalculatePinLocations();

            //setup setiap pin sebagai obstacle
            foreach (var c in board.Schema.Components)
            {
                foreach (var pin in c.Package.Pins)
                {
                    var key = new Tuple<Component, string>(c, pin.Name);
                    var value = new List<IntPoint>(85);

                    var pos = board.GetPinLocation(c, pin.Name);
                    var pp = PointToIntPoint(pos);

                    var z = span - 1;
                    for (var i = -z; i <= z; i++)
                    {
                        var si = i < 0 ? -i : i;
                        var hSpan = z1[si];
                        var hz = hSpan - 1;
                        for (var j = -hz; j <= hz; j++)
                        {
                            var p = new IntPoint(pp.X+i, pp.Y+j);
                            if (p.X >= 0 && p.X < workspace.Width && p.Y >= 0 && p.Y < workspace.Height)
                            {
                                workspace[p] = -1;
                                value.Add(p);
                            }
                        }
                    }

                    _pinObs.Add(key, value);
                }
            }
        }

        enum PointType
        {
            None,
            First,
            Last,
            Horizontal,
            Vertikal,
            Corner
        }

        private PointType GetNodeType(IList<IntPoint> pts, int index)
        {
            if(index == 0)
                return PointType.First;

            if(index == pts.Count-1)
                return PointType.Last;

            var cPoint = pts[index];
            var nPoint = pts[index + 1];

            if (cPoint.X != nPoint.X && cPoint.Y != nPoint.Y)
            {
                return PointType.Corner;
            }

            if (cPoint.X == nPoint.X)
            {
                return PointType.Horizontal;
            }

            if (cPoint.Y == nPoint.Y)
            {
                return PointType.Vertikal;                
            }

            return PointType.None;            
        }

        public void Route(Board board)
        {
            board.CalculatePinLocations();
            var workspace = new RouterWorkspace(board, CellSize);
            SetupWorkspace(workspace);

            var zc = board.Schema.Connections.Count;
            var zl = board.Schema.Connections.ToList();

            for (var i = 0; i < 10; i++)
            {
                var z = zl[i];

                var pts = new HashSet<IntPoint>();
                foreach (var z1 in z.Pins)
                {
                    var pos = board.GetPinLocation(z1.Component, z1.Pin);
                    var rList = _pinObs[new Tuple<Component, string>(z1.Component, z1.Pin)];
                    foreach (var x in rList)
                    {
                        workspace[x] = 0;
                    }

                    pts.Add(PointToIntPoint(pos));
                }

                var r = new LeeMultipointRouter(workspace, pts);
                var res = r.Route();
                _logger.Debug($"Routing result {res}");
                
                foreach (var z1 in z.Pins)
                {
                    var rList = _pinObs[new Tuple<Component, string>(z1.Component, z1.Pin)].ToList();                    
                    for(var x = 0; x < rList.Count; x++)
                    {
                        workspace[rList[x]] = -1;
                    }
                }

                if (res)
                {
                    var l = r.Trace.ToList();
                    for(var ixx = 0; ixx < l.Count; ixx++)
                    {
                        var tWidth = 1;
                        var xxx = (int)Math.Round(tWidth / CellSize);

                        var hmin = -xxx;
                        var hmax = xxx;

                        var vmin = -xxx;
                        var vmax = xxx;
                     
                        for (var xh = hmin; xh <= hmax; xh++)
                        {
                            for (var xv = vmin; xv <= vmax; xv++)
                            {
                                var xx = l[ixx].X + xh;
                                var yy = l[ixx].Y + xv;

                                if (xx < 0 || yy < 0 || xx >= workspace.Width || yy >= workspace.Height)
                                {
                                    continue;
                                }

                                var p = new IntPoint(xx, yy);
                                workspace[p] = -1;
                            }
                        }
                    }
                }

                var r2 = r.TraceNodes
                .Select(
                x =>
                {
                    return (IList<Point>)x.Select(IntPointToPoint).ToList();
                }).ToList();

                board._traces.Add(r2);
                board._cellSize = CellSize;
                board._wValues = workspace._internalData;
            }
        }
    }
}