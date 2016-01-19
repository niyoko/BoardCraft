namespace BoardCraft.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Models;
    using NLog;
    using Drawing;
    using Drawing.PinStyles;

    public class Router
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
    
        private double TraceWidth { get; }
        private double Clearance { get; }
        private readonly double CellSize;
        private const int Divider = 4;

        public Router(double traceWidth, double clearance)
        {
            TraceWidth = traceWidth;
            Clearance = clearance;
            CellSize = (traceWidth + clearance) / Divider;
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
            var pin = component.Package.Pins.SingleOrDefault(x => x.Name == pinName);
            if (pin == null)
                yield break;

            var pos = board.GetPinLocation(component, pinName);
            var pp = PointToIntPoint(pos);

            var c = pin.Style as CirclePinStyle;
            if(c != null)
            {
                //bottom
                var rad = (c.PadDiameter / 2) + Clearance + ((TraceWidth - CellSize) / 2);
                var rad2 = (int)(Math.Ceiling(rad / CellSize));
                var pts = RoutingHelper.GetPointsInCircle(pp, rad2);
                foreach (var p in pts)
                {
                    yield return new LPoint(WorkspaceLayer.BottomLayer, p);
                }

                //top
                var rad3 = c.DrillDiameter / 2;
                var rad4 = (int)(Math.Ceiling(rad3 / CellSize));
                var pts2 = RoutingHelper.GetPointsInCircle(pp, rad4);
                foreach (var p2 in pts2)
                {
                    yield return new LPoint(WorkspaceLayer.TopLayer, p2);
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
                //var tWidth = 40;
                var xxx = Divider-1;

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
                Debug.WriteLine("Id is " + z.Id);

                var r = new LeeMultipointRouter(workspace, pts);
                var res = r.Route();
                if (res)
                {
                    //return;
                }
                else
                {
#if DEBUG
                    board._wValues = workspace._data;
                    board._cellSize = CellSize;
                    board._targets = r._target;
                    Debug.WriteLine("Routing failed");
                    return;
#endif
                }
                var l = SquareBuffer(r.Trace);
                var lx = l.Where(x=>workspace.IsPointValid(x.Point)).ToList();
                workspace.SetTrackObstacle(z, lx);

                var r2 = r.TraceNodes
                    .Select(x => (IList<TraceNode>)x.Select(LNodeToTraceNode).ToList())
                    .ToList();

                board._traces.Add(r2);
            }

            Debug.WriteLine("Routing success");
        }
    }
}