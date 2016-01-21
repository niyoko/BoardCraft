namespace BoardCraft.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Models;
    using Drawing.PinStyles;

    public class Router
    {    
        private double TraceWidth { get; }
        private double Clearance { get; }
        private int Divider { get; }
        private readonly double _cellSize;

        private readonly ISet<IntPoint> _traceBufferOffset;
        private readonly ISet<IntPoint> _viaObs; 
        public Router(double traceWidth, double clearance, int cellDivider)
        {
            TraceWidth = traceWidth;
            Clearance = clearance;
            Divider = cellDivider;

            _cellSize = (traceWidth + clearance) / Divider;
            
            var t = RoutingHelper.GetPointsInCircle(new IntPoint(0, 0),  Divider - 1);
            _traceBufferOffset = new HashSet<IntPoint>(t);

            const int viaRadi = 15;
            var realRadi = viaRadi + Clearance + ((TraceWidth - _cellSize) / 2);
            var r2 = (int)(Math.Ceiling(realRadi/_cellSize));

            var t2 = RoutingHelper.GetPointsInCircle(new IntPoint(0, 0), r2);
            _viaObs = new HashSet<IntPoint>(t2);
        }
        
        private IEnumerable<LPoint> GetObstacleForPin(Board board, ComponentPin pin, RouterWorkspace workspace)
        {
            var pos = board.GetPinLocation(pin);
            var pp = workspace.PointToIntPoint(pos);

            var c = pin.PackagePin.Style as CirclePinStyle;
            if(c != null)
            {
                //bottom
                var rad = (c.PadDiameter / 2) + Clearance + ((TraceWidth - _cellSize) / 2);
                var rad2 = (int)(Math.Ceiling(rad / _cellSize));
                var pts = RoutingHelper.GetPointsInCircle(pp, rad2);
                foreach (var p in pts)
                {
                    yield return new LPoint(WorkspaceLayer.BottomLayer, p);
                }

                //top
                var rad3 = (c.DrillDiameter / 2) + Clearance + ((TraceWidth - _cellSize) / 2);
                var rad4 = (int)(Math.Ceiling(rad3 / _cellSize));
                var pts2 = RoutingHelper.GetPointsInCircle(pp, rad4);
                foreach (var p2 in pts2)
                {
                    yield return new LPoint(WorkspaceLayer.TopLayer, p2);
                }
            }

            var csq = pin.PackagePin.Style as SquarePinStyle;
            if (csq != null)
            {
                //bottom
                var o1 = (csq.SquareSide/2) + Clearance + ((TraceWidth - _cellSize) / 2);
                var o = (int) (Math.Ceiling(o1/_cellSize));
                for (var i = -o; i <= o; i++)
                {
                    for (var j = -o; j <= o; j++)
                    {
                        yield return new LPoint(WorkspaceLayer.BottomLayer, new IntPoint(pp.X + i, pp.Y + j));
                    }
                }

                //top
                var rad3 = (csq.DrillDiameter / 2) + Clearance + ((TraceWidth - _cellSize) / 2);
                var rad4 = (int)(Math.Ceiling(rad3 / _cellSize));
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
                foreach (var pin in c.Pins)
                {
                    var obs = GetObstacleForPin(board, pin, workspace);
                    obs = obs.Where(x => workspace.IsPointValid(x.Point));
                    workspace.PinObstacle[pin] = new HashSet<LPoint>(obs);
                }
            }
        }

        private void RevertRoute(Connection connection)
        {
            
        }

        private ISet<LPoint> Buffer(ISet<LPoint> traces, RouterWorkspace workspace)
        {
            var clist = (List<LPoint>) null;
            var set = new HashSet<LPoint>();
            foreach (var t in traces)
            {
                var nl = new List<LPoint>();
                foreach (var o in _traceBufferOffset)
                {
                    var cl = RoutingHelper.OffsetPoint(t, o);
                    if (!workspace.IsPointValid(cl.Point))
                        continue;
                
                    if (clist == null || !clist.Contains(cl))
                    {
                        set.Add(cl);
                    }

                    nl.Add(cl);
                }
                clist = nl;
            }

            return set;
        }

        private IEnumerable<LPoint> GetObstacleForVia(IntPoint point, RouterWorkspace workspace)
        {
            var enu = _viaObs.Select(x => new IntPoint(x.X + point.X, x.Y + point.Y));
            enu = enu.Where(workspace.IsPointValid);
            var enu2 = enu.SelectMany(
                x => new[] { WorkspaceLayer.TopLayer, WorkspaceLayer.BottomLayer }, 
                (a, b) => new LPoint(b, a)
            );

            return enu2;
        } 

        private bool RouteConnection(Board board, RouterWorkspace workspace, Connection connection)
        {
            var pinLoc = connection.Pins
                    .Select(x => board.GetPinLocation(x.Pin))
                    .Select(workspace.PointToIntPoint);

            var pts = new HashSet<IntPoint>(pinLoc);

            //setup worskspace
            workspace.SetupWorkspaceForRouting(connection);

            //doing route
            var r = new LeeMultipointRouter(workspace, pts);
            var result = r.Route();

            //if routing fails, return false
            if (!result)
                return false;

            workspace.TraceSegments[connection] = r.TraceSegments;
            workspace.TracePoint[connection] = r.TracePoints;
            workspace.Vias[connection] = r.Vias;

            //set obstacles for next routing
            var l = Buffer(r.TracePoints, workspace);
            workspace.TraceObstacle[connection] = l;

            var l1 = new List<LPoint>();
            foreach (var v in r.Vias)
            {
                var viaObs = GetObstacleForVia(v, workspace);
                l1.AddRange(viaObs);
            }
            workspace.ViaObstacle[connection] = new HashSet<LPoint>(l1);           

            return true;
        }

        public void Route(Board board)
        {
            board.CalculatePinLocations();
            var workspace = new RouterWorkspace(board, _cellSize);
            SetupWorkspace(workspace);

            var distances = board.CalculatePinDistances();

            var zl = distances
                .OrderBy(x => x.Key.Pins.Count)
                .ThenBy(x => x.Value.Max)
                .Select(x => x.Key)
                .ToList();

            foreach (var z in zl)
            {
                var result = RouteConnection(board, workspace, z);
#if DEBUG
                Debug.WriteLine($"Routing {z.Id} Result : {(result?"Success":"Fail")}");
#endif
            }

            workspace.WriteToBoard();
        }
    }
}