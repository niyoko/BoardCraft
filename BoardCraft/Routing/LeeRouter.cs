using BoardCraft.Helpers;
using System.Collections.Generic;

namespace BoardCraft.Routing
{
    using System;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Drawing;

    internal class LeeRouter
    {
        private readonly RouterWorkspace _workspace;
        private readonly ISet<LPoint> _starts;
        private readonly ISet<LPoint> _targets;

        private readonly List<LPoint> _internalTracePoint;
        private readonly List<AbstractTraceSegment> _internalTraceSegments;
        private readonly HashSet<IntPoint> _internalVias;
        
        public IList<LPoint> TracePoints { get; }
        public IList<AbstractTraceSegment> TraceSegments { get; }
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
            const int radi = 3;
            var t = RoutingHelper.GetPointsInCircle(new IntPoint(0, 0), radi);
            ViaBufferOffset = new HashSet<IntPoint>(t);
        }

        public LeeRouter(RouterWorkspace workspace, ISet<LPoint> starts, ISet<LPoint> targets)
        {
            _workspace = workspace;
            _starts = starts;
            _targets = targets;
            _internalTracePoint = new List<LPoint>();
            _internalTraceSegments = new List<AbstractTraceSegment>();
            _internalVias = new HashSet<IntPoint>();

            TracePoints = new ReadOnlyCollection<LPoint>(_internalTracePoint);
            TraceSegments = new ReadOnlyCollection<AbstractTraceSegment>(_internalTraceSegments);
            Vias = new ReadOnlySet<IntPoint>(_internalVias);
        }

        public bool Route()
        {
            Init();
            var end = ExpandWave();
            if (end == null)
            {
#if FALSE
                var botIm = new Bitmap(_workspace.Width, _workspace.Height);

                _workspace.Dump(botIm);

                foreach (var t in _targets)
                {
                    var d = $"{t.Layer},{t.Point.X},{t.Point.Y} ";
                    d += $"Top : {_workspace[new LPoint(WorkspaceLayer.TopLayer, t.Point)]} ";
                    d += $"Bot : {_workspace[t]} ";
                    d += $"MetaTop : {_workspace.GetMetadata(new LPoint(WorkspaceLayer.TopLayer, t.Point))} ";
                    d += $"MetaBot : {_workspace.GetMetadata(t)}";
                    Debug.WriteLine(d);

                    var ti = new IntPoint(t.Point.X, _workspace.Height -1 - t.Point.Y);
                    var b = RoutingHelper.GetPointsInCircle(ti, 3);
                    foreach (var v in b)
                    {
                        botIm.SetPixel(v.X, v.Y, Color.Gainsboro);
                    }
                }

                var di = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                var ui = Guid.NewGuid().ToString("N").Substring(0, 5);

                botIm.Save($@"D:\{di}-{ui}.png");
#endif
                return false;
            }

            Backtrace(end.Value);
            ConvertToVector();

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
            _workspace.ClearData();
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

        private static IEnumerable<IntPoint> ConvertToVector2(IEnumerable<IntPoint> points)
        {
            int pdx = 0, pdy = 0;
            var pp = default(IntPoint);
            var i = 0;

            foreach (var p in points)
            {
                if (i == 0)
                {
                    pp = p;
                    i++;
                    continue;
                }

                var dx = p.X - pp.X;
                var dy = p.Y - pp.Y;

                if (dx != pdx || dy != pdy)
                {
                    yield return pp;
                }

                pp = p;
                i++;
                pdx = dx;
                pdy = dy;
            }

            if (i > 0)
            {
                yield return pp;
            }
        }

        private void ConvertToVector()
        {
            if (_internalTracePoint.Count < 2)
            {
                throw new InvalidOperationException("Cannot route zero or single point");
            }

            IList<IntPoint> cl = new List<IntPoint>();
            for (var i = 0; i < _internalTracePoint.Count; i++)
            {
                if (i != 0)
                {
                    var pLayer = _internalTracePoint[i - 1].Layer;
                    var cLayer = _internalTracePoint[i].Layer;
                    
                    if (pLayer != cLayer)
                    {
                        //changing in layers
                        var pPoint = _internalTracePoint[i - 1].Point;
                        var cPoint = _internalTracePoint[i].Point;

                        if (pPoint != cPoint)
                        {
                            throw new Exception("Failed to change raster to vector");
                        }

                        _internalVias.Add(cPoint);

                        var nodes = ConvertToVector2(cl);
                        var s = new AbstractTraceSegment(pLayer, nodes);
                        
                        _internalTraceSegments.Add(s);

                        cl = new List<IntPoint>();
                    }
                }

                cl.Add(_internalTracePoint[i].Point);

                if (i == _internalTracePoint.Count - 1)
                {
                    var nodes = ConvertToVector2(cl);
                    var s = new AbstractTraceSegment(_internalTracePoint[i].Layer, nodes);
                    _internalTraceSegments.Add(s);
                }
            }
        }

        private void Backtrace(IntPoint start)
        {
            var c = new LPoint(WorkspaceLayer.BottomLayer, start);
            _internalTracePoint.Add(c);
            while (true)
            {
                var cval = _workspace[c];
                if (cval == 1)
                {
                    //backtrace finished
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
                _internalTracePoint.Add(mn);
                c = mn;
            }
        }
    }
}
