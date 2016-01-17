namespace BoardCraft.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Models;
    using NLog;
    using Drawing;

    public class Router
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
        private double TraceWidth { get; }
        private double MinimumSpacing { get; }
        private static double CellSize = 10;

        private static List<int> z1; 

        static Router()
        {
            const double pinRad = 50;
            var span = (int)Math.Round(pinRad / CellSize + 1);

            z1 = new List<int>(span);
            for (var i = 0; i < span; i++)
            {
                var th = Math.Acos((double)i / span);
                var leftSpan = (int)(Math.Round(Math.Sin(th) * span));
                z1.Add(leftSpan);
            }
        }

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

        private IEnumerable<IntPoint> GetObstacleForPin(Board board, Component component, string pinName)
        {
            var pos = board.GetPinLocation(component, pinName);
            var pp = PointToIntPoint(pos);

            var z = z1.Count - 1;
            for (var i = -z; i <= z; i++)
            {
                var si = i < 0 ? -i : i;
                var hSpan = z1[si];
                var hz = hSpan - 1;
                for (var j = -hz; j <= hz; j++)
                {
                    var p = new IntPoint(pp.X + i, pp.Y + j);
                    yield return p;
                }
            }
        } 

        private void SetupWorkspace(RouterWorkspace workspace)
        {
            var board = workspace.Board;                       
            board.CalculatePinLocations();

            //setup setiap pin sebagai obstacle
            foreach (var c in board.Schema.Components)
            {
                foreach (var pin in c.Package.Pins)
                {
                    var obs = GetObstacleForPin(board, c, pin.Name);
                    obs = obs.Where(workspace.IsPointValid);                    
                    workspace.SetPinObstacle(c, pin.Name, obs);
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

        private ISet<IntPoint> SquareBuffer(ISet<IntPoint> tracks)
        {
            var r = new HashSet<IntPoint>();
            foreach (var t in tracks)
            {
                var tWidth = 40;
                var xxx = (int)Math.Round(tWidth / CellSize);

                var hmin = -xxx;
                var hmax = xxx;

                var vmin = -xxx;
                var vmax = xxx;

                for (var xh = hmin; xh <= hmax; xh++)
                {
                    for (var xv = vmin; xv <= vmax; xv++)
                    {
                        var xx = t.X + xh;
                        var yy = t.Y + xv;

                        var p = new IntPoint(xx, yy);
                        r.Add(p);
                    }
                }
            }

            return r;
        } 

        public void Route(Board board)
        {
            board.CalculatePinLocations();
            var workspace = new RouterWorkspace(board, CellSize);
            SetupWorkspace(workspace);

            var distances = board.CalculatePinDistances();

            var zl = distances.OrderBy(x => x.Value.Max)
                    .Select(x => x.Key)
                    .Take(10)
                    .ToList();

            var successRouting = 0;
            var failedRouting = 0;

            for (var i = 0; i < zl.Count; i++)
            {
                var z = zl[i];

                var pinLoc = z.Pins
                    .Select(x => board.GetPinLocation(x.Component, x.Pin))
                    .Select(PointToIntPoint);

                var pts = new HashSet<IntPoint>(pinLoc);

                workspace.SetupWorkspaceForRouting(z);
                var r = new LeeMultipointRouter(workspace, pts);
                var res = r.Route();

                if (res)
                {
                    successRouting++;
                }
                else
                {
                    failedRouting++;
                }

                //if (res)
                //{
                    var l = SquareBuffer(r.Trace);
                    
                    var lx = l.Where(workspace.IsPointValid).ToList();
                    Debug.WriteLine($"From {r.Trace.Count} become {l.Count} and then {lx.Count}");

                    workspace.SetTrackObstacle(z, WorkspaceLayer.BottomLayer, lx);
                //}

                var r2 = r.TraceNodes
                .Select(
                x =>
                {
                    return (IList<Point>)x.Select(IntPointToPoint).ToList();
                }).ToList();

                board._traces.Add(r2);
            }

            Debug.WriteLine($"Success : {successRouting} Fail : {failedRouting}");

            //board._cellSize = CellSize;
            //board._wValues = workspace._internalData;
        }
    }
}