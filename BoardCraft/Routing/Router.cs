namespace BoardCraft.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Sockets;
    using Drawing;
    using Models;
    using Drawing.PinStyles;

    public class Router
    {    
        private double TraceWidth { get; }
        private double Clearance { get; }
        private int Divider { get; }
        private readonly double _cellSize;

        private readonly ISet<IntPoint> _trackBufferOffset;

        public Router(double traceWidth, double clearance, int cellDivider)
        {
            TraceWidth = traceWidth;
            Clearance = clearance;
            Divider = cellDivider;

            _cellSize = (traceWidth + clearance) / Divider;
            
            var t = RoutingHelper.GetPointsInCircle(new IntPoint(0, 0),  Divider - 1);
            _trackBufferOffset = new HashSet<IntPoint>(t);
        }
        
        private IEnumerable<LPoint> GetObstacleForPin(Board board, Component component, string pinName, RouterWorkspace workspace)
        {
            var pin = component.Package.Pins.SingleOrDefault(x => x.Name == pinName);
            if (pin == null)
                yield break;

            var pos = board.GetPinLocation(component, pinName);
            var pp = workspace.PointToIntPoint(pos);

            var c = pin.Style as CirclePinStyle;
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
                var rad3 = c.DrillDiameter / 2;
                var rad4 = (int)(Math.Ceiling(rad3 / _cellSize));
                var pts2 = RoutingHelper.GetPointsInCircle(pp, rad4);
                foreach (var p2 in pts2)
                {
                    yield return new LPoint(WorkspaceLayer.TopLayer, p2);
                }
            }

            var csq = pin.Style as SquarePinStyle;
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
                var rad3 = csq.DrillDiameter / 2;
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
                foreach (var pin in c.Package.Pins)
                {
                    var obs = GetObstacleForPin(board, c, pin.Name, workspace);
                    obs = obs.Where(x => workspace.IsPointValid(x.Point));
                    workspace.SetPinObstacle(c, pin.Name, obs);
                }
            }
        }

        private ISet<LPoint> Buffer(ISet<LPoint> tracks, RouterWorkspace workspace)
        {
            var clist = (List<LPoint>) null;
            var set = new HashSet<LPoint>();
            foreach (var t in tracks)
            {
                var nl = new List<LPoint>();
                foreach (var o in _trackBufferOffset)
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

        private static TraceNode LNodeToTraceNode(LPoint lp, RouterWorkspace workspace)
        {
            var p = workspace.IntPointToPoint(lp.Point);
            var l = lp.Layer == WorkspaceLayer.BottomLayer 
                    ? TraceLayer.BottomLayer 
                    : TraceLayer.TopLayer;
            return new TraceNode(p, l);
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

#if DEBUG
            int s = 0, f = 0;
#endif

            foreach (var z in zl)
            {
                var pinLoc = z.Pins
                    .Select(x => board.GetPinLocation(x.Component, x.Pin))
                    .Select(workspace.PointToIntPoint);

                var pts = new HashSet<IntPoint>(pinLoc);

                workspace.SetupWorkspaceForRouting(z);

                var r = new LeeMultipointRouter(workspace, pts);
                var res = r.Route();
                if (res)
                {
#if DEBUG
                    s++;
#endif
                }
                else
                {
#if DEBUG

                    f++;
#endif
                }

                var l = Buffer(r.Trace, workspace);
                workspace.SetTrackObstacle(z, l);

                var r2 = r.TraceNodes
                    .Select(x => (IList<TraceNode>)x.Select(y => LNodeToTraceNode(y, workspace)).ToList())
                    .ToList();

                var segments = new List<TraceSegment>();
                TraceSegment cs = null;
                for(var i = 0; i<r2.Count; i++)
                {
                    var xr = r2[i];
                    for (var j = 0; j < xr.Count;j++)
                    {
                        if (j == 0 || xr[j].Layer != xr[j - 1].Layer)
                        {
                            if (i != 0)
                            {
                                segments.Add(cs);
                            }

                            cs = new TraceSegment(xr[j].Layer);
                        }

                        cs.AddNode(xr[j].Point);
                    }
                }

                board.TraceSegments.Add(z, segments);
                //board.Traces.Add(r2);


                var v2 = r.Vias
                    .Select(x => workspace.IntPointToPoint(x))
                    .ToList();

                board.Vias.UnionWith(v2);
            }
#if DEBUG
            Debug.WriteLine($"Success : {s} Failed : {f}");
#endif
        }
    }
}