using BoardCraft.Helpers;
using System.Collections.Generic;

namespace BoardCraft.Routing
{
    using System.Collections.ObjectModel;

    internal class LeeRouter
    {
        private readonly RouterWorkspace _workspace;
        private readonly ISet<IntPoint> _starts;
        private readonly ISet<IntPoint> _targets;
        private readonly ISet<IntPoint> _internalTrack;
        private readonly IList<IntPoint> _internalNodes;

        public ISet<IntPoint> Track { get; }
        public IList<IntPoint> TrackNodes { get; } 

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

        public LeeRouter(RouterWorkspace workspace, ISet<IntPoint> starts, ISet<IntPoint> targets)
        {
            _workspace = workspace;
            _starts = starts;
            _targets = targets;
            _internalTrack = new HashSet<IntPoint>();
            _internalNodes = new List<IntPoint>();

            Track = new ReadOnlySet<IntPoint>(_internalTrack);
            TrackNodes = new ReadOnlyCollection<IntPoint>(_internalNodes);
        }

        public bool Route()
        {
            Init();
            var end = ExpandWave();
            if (end == null)
            {
                Clear();
                return false;
            }

            Backtrace(end.Value);
            Clear();

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
            var currentPoints = new List<IntPoint>(_starts);
            var end = (IntPoint?)null;

            while (true)
            {
                var next = new List<IntPoint>(100);
                foreach (var c in currentPoints)
                {
                    foreach (var z in neighbors)
                    {
                        var nx = c.X + z.Key.X;
                        var ny = c.Y + z.Key.Y;
                        var n = new IntPoint(nx, ny);

                        if (nx < 0 || nx >= _workspace.Width ||
                            ny < 0 || ny >= _workspace.Height)
                        {
                            continue;
                        }

                        var cpv = _workspace[c];
                        var sv = cpv + z.Value;
                        var cv = _workspace[n];

                        if (_workspace[n] == 0)
                        {
                            if (_targets.Contains(n))
                            {
                                //found
                                end = n;
                                break;
                            }
                        }

                        if (_workspace[n] == 0 || sv < cv)
                        {
                            _workspace[n] = sv;
                            next.Add(n);
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

        private void Clear()
        {
            var d0 = _workspace.Width;
            var d1 = _workspace.Height;

            for (var i = 0; i < d0; i++)
            {
                for (var j = 0; j < d1; j++)
                {
                    var p = new IntPoint(i, j);
                    if (_workspace[p] > 0)
                    {
                        _workspace[p] = 0;
                    }
                }
            }
        }

        private IntPoint Backtrace(IntPoint start)
        {
            var c = start;
            var dx = 0;
            var dy = 0;
            var pdx = 0;
            var pdy = 0;

            _internalTrack.Add(c);
            while (true)
            {
                if (_workspace[c] == 1)
                {

                    _internalNodes.Add(c);
                    return c;
                }

                var mn = c;
                var mVal = int.MaxValue;

                foreach (var z in neighbors)
                {
                    var nx = c.X + z.Key.X;
                    var ny = c.Y + z.Key.Y;
                    var n = new IntPoint(nx, ny);

                    var val = _workspace[n];
                    if (val <= 0)
                    {
                        continue;
                    }

                    if (val < mVal)
                    {
                        mn = new IntPoint(nx, ny);
                        mVal = val;
                    }
                }

                dx = mn.X - c.X;
                dy = mn.Y - c.Y;

                if (!(dx == pdx && dy == pdy))
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
