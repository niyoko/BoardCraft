namespace BoardCraft.Routing
{
    using System.Diagnostics;
    using BoardCraft.Helpers;
    using System.Collections.Generic;
    using System.Linq;

    internal class LeeMultipointRouter
    {
        private readonly ISet<IntPoint> _pins;
        private ISet<IntPoint> _pinsWrapper;

        internal RouterWorkspace Workspace { get; }

        public ISet<LPoint> Trace { get; }
        public ICollection<IList<LPoint>> TraceNodes { get; } 

        public LeeMultipointRouter(RouterWorkspace workspace, ISet<IntPoint> pins)
        {
            Workspace = workspace;
            _pins = pins;

            Trace = new HashSet<LPoint>();
            TraceNodes = new List<IList<LPoint>>();
        }

        public ISet<IntPoint> Pins
        {
            get
            {
                if (_pinsWrapper == null)
                {
                    _pinsWrapper = new ReadOnlySet<IntPoint>(_pins);
                }

                return _pinsWrapper;
            }
        }

        public bool Route()
        {
            var rp = Pins.ToArray()[0];
            Trace.Add(new LPoint(WorkspaceLayer.BottomLayer, rp));

            while (!IsFinished())
            {
                var pins = Pins.Select(x => new LPoint(WorkspaceLayer.BottomLayer, x));
                var target = new HashSet<LPoint>(pins);
                target.ExceptWith(Trace);
                var singleRouter = new LeeRouter(Workspace, Trace, target);
                var rResult = singleRouter.Route();
                if (!rResult)
                {
                    return false;
                }

                Trace.UnionWith(singleRouter.Track);
                TraceNodes.Add(singleRouter.TrackNodes);
            }

            return true;
        }

        private bool IsFinished()
        {
            var t = Trace.Where(x => x.Layer == WorkspaceLayer.BottomLayer)
                .Select(x => x.Point)
                .ToList();

            return Pins.All(x => t.Contains(x));
        }
    }
}
