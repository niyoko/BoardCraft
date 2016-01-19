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
        private readonly ISet<IntPoint> _internalVias; 
        

        public ISet<LPoint> Track { get; }
        public IList<LPoint> TrackNodes { get; }
        public ISet<IntPoint> Vias { get; } 

        private static readonly Dictionary<IntPoint, int> Neighbors = new Dictionary<IntPoint, int>
        {
            { new IntPoint(-1, 0), 10 },    
            { new IntPoint(0, -1), 10 },
            { new IntPoint(1, 0), 10 },
            { new IntPoint(0, 1), 10 },
            { new IntPoint(1, 1), 14 },
            { new IntPoint(1, -1), 14 },
            { new IntPoint(-1, 1), 14 },
            { new IntPoint(-1, -1), 14 }
        };

        private const int ViaCost = 50;

        private static readonly ISet<IntPoint> ViaBufferOffset;

        static LeeRouter()
        {
            const int radi = 5;
            var t = RoutingHelper.GetPointsInCircle(new IntPoint(0, 0), radi);
            ViaBufferOffset = new HashSet<IntPoint>(t);
        }

        public LeeRouter(RouterWorkspace workspace, ISet<LPoint> starts, ISet<LPoint> targets)
        {
            _workspace = workspace;
            _starts = starts;
            _targets = targets;
            _internalTrack = new HashSet<LPoint>();
            _internalNodes = new List<LPoint>();
            _internalVias = new HashSet<IntPoint>();

            Track = new ReadOnlySet<LPoint>(_internalTrack);
            TrackNodes = new ReadOnlyCollection<LPoint>(_internalNodes);
            Vias = new ReadOnlySet<IntPoint>(_internalVias);
        }

        public bool Route()
        {
            Init();
#if DEBUG
            var sw = Stopwatch.StartNew();
#endif
            var end = ExpandWave();
#if DEBUG
            sw.Stop();
            Debug.WriteLine("Expand wave : " + sw.ElapsedMilliseconds);
#endif
            if (end == null)
            {
                Debug.WriteLine("Expand wave fail");
                return false;
            }
#if DEBUG
            sw = Stopwatch.StartNew();
#endif
            Backtrace(end.Value);
#if DEBUG
            sw.Stop();
            Debug.WriteLine("Backtrace : " + sw.ElapsedMilliseconds);
#endif
            return true;
        }

        private bool CanCreateVia(IntPoint point)
        {
            foreach (var p in ViaBufferOffset)
            {
                var p2 = new IntPoint(point.X + p.X, point.Y + p.Y);
                if (!_workspace.IsPointValid(p2))
                {
                    return false;
                }

                var bb = new LPoint(WorkspaceLayer.BottomLayer, p2);
                var mb = _workspace.GetMetadata(bb);
                if (mb == CellMetadata.Obstacle || mb == CellMetadata.SuspendedObstacle)
                    return false;

                var bt = new LPoint(WorkspaceLayer.TopLayer, p2);
                var mt = _workspace.GetMetadata(bt);
                if (mt == CellMetadata.Obstacle || mt == CellMetadata.SuspendedObstacle)
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
                    foreach (var z in Neighbors)
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
                        var nmeta = _workspace.GetMetadata(n);

                        if (cv == 0 && (nmeta == CellMetadata.Freelane || nmeta == CellMetadata.SuspendedObstacle))
                        {
                            if (_targets.Contains(n))
                            {
                                _workspace[n] = sv;
                                end = n.Point;
                                break;
                            }
                        }

                        if ((nmeta == CellMetadata.Freelane || nmeta == CellMetadata.SuspendedObstacle) && (cv == 0 || sv < cv))
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
                    var nmeta2 = _workspace.GetMetadata(nl);

                    if ((nmeta2 == CellMetadata.Freelane || nmeta2 == CellMetadata.SuspendedObstacle) && (cvl == 0 || spvl < cvl))
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

        private void Backtrace(IntPoint start)
        {
            var c = new LPoint(WorkspaceLayer.BottomLayer, start);
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
                    return;
                }

                var mn = c;
                var mVal = cval;

                foreach (var z in Neighbors)
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

                var via = false;
                //layer move
                if (CanCreateVia(c.Point))
                {
                    var nlayer = c.Layer == WorkspaceLayer.TopLayer
                        ? WorkspaceLayer.BottomLayer
                        : WorkspaceLayer.TopLayer;
                    var nl = new LPoint(nlayer, c.Point);

                    var val2 = _workspace[nl];

                    if (val2 > 0 && val2 < mVal)
                    {
                        mn = nl;
                        mVal = val2;
                        via = true;
                    }
                }

                if (mVal >= cval)
                {
                    throw new Exception("Backtrace stuck. Something wrong");
                }
#if DEBUG
                var sel = cval - mVal;
                if (sel != 10 && sel != 14 && sel != 50)
                {
                    Debug.WriteLine("Unnatural trace found - " + sel);
                }
#endif
                var dx = mn.Point.X - c.Point.X;
                var dy = mn.Point.Y - c.Point.Y;
                var dl = (int)mn.Layer - (int)c.Layer;

                if (dx != pdx || dy != pdy || dl != pdl)
                {
                    _internalNodes.Add(c);
                }
                
                _internalTrack.Add(mn);
                if (via)
                {
                    _internalVias.Add(mn.Point);
                }

                pdx = dx;
                pdy = dy;
                pdl = dl;
                c = mn;
            }
        }
    }
}
