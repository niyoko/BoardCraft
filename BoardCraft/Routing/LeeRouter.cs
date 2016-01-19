using BoardCraft.Helpers;
using System.Collections.Generic;

namespace BoardCraft.Routing
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;

    internal class LeeRouter
    {
        private readonly RouterWorkspace _workspace;
        private readonly ISet<LPoint> _starts;
        private readonly ISet<LPoint> _targets;
        private readonly ISet<LPoint> _internalTrack;
        private readonly IList<LPoint> _internalNodes;

        public ISet<LPoint> Track { get; }
        public IList<LPoint> TrackNodes { get; }

        private static readonly Dictionary<IntPoint, int> neighbors = new Dictionary<IntPoint, int>
        {
            { new IntPoint(-1, 0), 10 },    
            { new IntPoint(0, -1), 10 },
            { new IntPoint(1, 0), 10 },
            { new IntPoint(0, 1), 10 },
            { new IntPoint(1, 1), 15 },
            { new IntPoint(1, -1), 15 },
            { new IntPoint(-1, 1), 15 },
            { new IntPoint(-1, -1), 15 }
        };

        private static readonly int ViaCost = 30;

        public LeeRouter(RouterWorkspace workspace, ISet<LPoint> starts, ISet<LPoint> targets)
        {
            _workspace = workspace;
            _starts = starts;
            _targets = targets;
            _internalTrack = new HashSet<LPoint>();
            _internalNodes = new List<LPoint>();

            Track = new ReadOnlySet<LPoint>(_internalTrack);
            TrackNodes = new ReadOnlyCollection<LPoint>(_internalNodes);
        }

        public bool Route()
        {
            Init();
            var end = ExpandWave();
            if (end == null)
            {
                return false;
            }

            Backtrace(end.Value);
            return true;
        }

        private bool CanCreateVia(IntPoint point)
        {
            return true;
            var viaClearance = 25;
            var pts = RoutingHelper.GetPointsInCircle(point, viaClearance);

            foreach (var p in pts)
            {
                if (!_workspace.IsPointValid(p))
                {
                    return false;
                }

                if (_workspace[new LPoint(WorkspaceLayer.BottomLayer, p)] < 0)
                    return false;

                if (_workspace[new LPoint(WorkspaceLayer.BottomLayer, p)] < 0)
                    return false;
            }

            return true;
        }

        private void Init()
        {
            foreach (var s in _starts)
            {
                _workspace[s] = 1;
            }
        }

        private IntPoint? ExpandWave()
        {
            var currentPoints = new List<LPoint>(_starts.Count);
            currentPoints.AddRange(_starts);

            var end = (IntPoint?)null;
            while (true)
            {
                var next = new List<LPoint>(100);
                foreach (var c in currentPoints)
                {
                    foreach (var z in neighbors)
                    {
                        var nx = c.Point.X + z.Key.X;
                        var ny = c.Point.Y + z.Key.Y;
                        var np = new IntPoint(nx, ny);
                        if (!_workspace.IsPointValid(np))
                        {
                            continue;
                        }

                        var n = new LPoint(c.Layer, np);

                        var cpv = _workspace[c];
                        var sv = cpv + z.Value;
                        var cv = _workspace[n];

                        if (_workspace[n] == 0)
                        {
                            if (_targets.Contains(n))
                            {
                                _workspace[n] = sv;
                                end = n.Point;
                                break;
                            }
                        }

                        if (cv == 0 || sv < cv)
                        {
                            _workspace[n] = sv;
                            next.Add(n);
                        }
                    }

                    //layer move
                    var nlayer = c.Layer == WorkspaceLayer.TopLayer
                        ? WorkspaceLayer.BottomLayer
                        : WorkspaceLayer.TopLayer;
                    var nl = new LPoint(nlayer, c.Point);
                    var cpvl = _workspace[c];
                    var spvl = cpvl + ViaCost;
                    var cvl = _workspace[nl];

                    if (cvl == 0 || spvl < cvl)
                    {
                        if (CanCreateVia(c.Point))
                        {
                            _workspace[nl] = spvl;
                            next.Add(nl);
                        }
                    }
                }

                if (end != null || next.Count == 0)
                {
                    break; 
                }

                currentPoints = next;
            }

            return end;
        }

        private IntPoint Backtrace(IntPoint start)
        {
            var c = new LPoint(WorkspaceLayer.BottomLayer, start);
            var dx = 0;
            var dy = 0;
            var dl = 0;
            var pdx = 0;
            var pdy = 0;
            var pdl = 0;

            _internalTrack.Add(c);
            while (true)
            {
                var cval = _workspace[c];
                if (cval == 1)
                {
                    _internalNodes.Add(c);
                    return c.Point;
                }

                var mn = c;
                var mVal = cval;

                foreach (var z in neighbors)
                {
                    var nx = c.Point.X + z.Key.X;
                    var ny = c.Point.Y + z.Key.Y;
                    var np = new IntPoint(nx, ny);

                    if (_workspace.IsPointValid(np))
                    {
                        var n = new LPoint(c.Layer, np);
                        var val = _workspace[n];
                        if (val > 0 && val < mVal)
                        {
                            mn = n;
                            mVal = val;
                        }
                    }
                }

                //layer move
                var nlayer = c.Layer == WorkspaceLayer.TopLayer
                    ? WorkspaceLayer.BottomLayer
                    : WorkspaceLayer.TopLayer;
                var nl = new LPoint(nlayer, c.Point);

                var val2 = _workspace[nl];

                if (val2 > 0 && val2 < mVal)
                {
                    mn = nl;
                    mVal = val2;
                }

                if (mVal >= cval)
                {
                    throw new Exception("Backtrace stuck. Something wrong");
                }
                
                dx = mn.Point.X - c.Point.X;
                dy = mn.Point.Y - c.Point.Y;
                dl = (int)mn.Layer - (int)c.Layer;

                if (dx != pdx || dy != pdy || dl != pdl)
                {
                    _internalNodes.Add(c);
                }
                
                _internalTrack.Add(mn);

                pdx = dx;
                pdy = dy;
                c = mn;
            }
        }
    }
}
