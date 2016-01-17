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

        private IEnumerable<LPoint> GetObstacleForPin(Board board, Component component, string pinName)
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
                    yield return new LPoint(WorkspaceLayer.TopLayer, p);
                    yield return new LPoint(WorkspaceLayer.BottomLayer, p);
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
                    obs = obs.Where(x => workspace.IsPointValid(x.Point));
                    workspace.SetPinObstacle(c, pin.Name, obs);
                }
            }
        }       

        private IList<LPoint> SquareBuffer(ISet<LPoint> tracks)
        {
            var r = new List<LPoint>();
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
                        var xx = t.Point.X + xh;
                        var yy = t.Point.Y + xv;

                        var p = new LPoint(t.Layer, new IntPoint(xx, yy));
                        r.Add(p);
                    }
                }
            }

            return r;
        }

        TraceNode LNodeToTraceNode(LPoint lp)
        {
            var p = IntPointToPoint(lp.Point);
            var l = lp.Layer == WorkspaceLayer.BottomLayer 
                    ? TraceLayer.BottomLayer 
                    : TraceLayer.TopLayer;
            return new TraceNode(p, l);
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

                Debug.WriteLine($"Routing {z.Id}");
                var sw = Stopwatch.StartNew();
                var res = r.Route();
                sw.Stop();
                var _z = sw.ElapsedMilliseconds;
                Debug.WriteLine($"Routed {z.Id} - {_z}");

                if (res)
                {
                    successRouting++;
                }
                else
                {
                    Debug.WriteLine($"Fail to route {z.Id}");
                    failedRouting++;
                }

                Debug.WriteLine($"Building buffer {z.Id}");
                sw.Restart();
                var l = SquareBuffer(r.Trace);                    
                sw.Stop();
                _z = sw.ElapsedMilliseconds;
                Debug.WriteLine($"Buffer builded at {_z}");
                sw.Restart();
                var lx = l.Where(x=>workspace.IsPointValid(x.Point)).ToList();
                sw.Stop();
                _z = sw.ElapsedMilliseconds;
                Debug.WriteLine($"Checked for invalid {_z}");

                workspace.SetTrackObstacle(z, lx);

                var r2 = r.TraceNodes
                    .Select(x => (IList<TraceNode>)x.Select(LNodeToTraceNode).ToList())
                    .ToList();

                board._traces.Add(r2);
            }
        }
    }
}