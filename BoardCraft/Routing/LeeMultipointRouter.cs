namespace BoardCraft.Routing
{
    using Helpers;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    internal class LeeMultipointRouter
    {
        private readonly ISet<LPoint> _pins;
        private ISet<LPoint> _pinsWrapper;

        internal RouterWorkspace Workspace { get; }

        private readonly HashSet<LPoint> _internalTracePoint;
        private readonly List<AbstractTraceSegment> _internalTraceSegments;
        private readonly HashSet<IntPoint> _internalVias;

        public ISet<LPoint> TracePoints { get; }
        public ICollection<AbstractTraceSegment> TraceSegments { get; }
        public ISet<IntPoint> Vias { get; }

        public LeeMultipointRouter(RouterWorkspace workspace, ISet<IntPoint> pins)
        {
            Workspace = workspace;
            _pins = new HashSet<LPoint>(pins.Select(x=> new LPoint(WorkspaceLayer.BottomLayer, x)));

            _internalTracePoint = new HashSet<LPoint>();
            _internalTraceSegments = new List<AbstractTraceSegment>();
            _internalVias = new HashSet<IntPoint>();

            TracePoints = new ReadOnlySet<LPoint>(_internalTracePoint);
            TraceSegments = new ReadOnlyCollection<AbstractTraceSegment>(_internalTraceSegments);
            Vias = new ReadOnlySet<IntPoint>(_internalVias);
        }

        public ISet<LPoint> Pins => _pinsWrapper ?? (_pinsWrapper = new ReadOnlySet<LPoint>(_pins));

        public bool Route()
        {
            var target = new HashSet<LPoint>(Pins);

            var rp = Pins.First();
            _internalTracePoint.Add(rp);

            target.ExceptWith(_internalTracePoint);

            while (target.Count > 0)
            {
                var singleRouter = new LeeRouter(Workspace, _internalTracePoint, target);
                var rResult = singleRouter.Route();
                if (!rResult)
                {
                    return false;
                }

                _internalTracePoint.UnionWith(singleRouter.TracePoints);
                _internalVias.UnionWith(singleRouter.Vias);
                _internalTraceSegments.AddRange(singleRouter.TraceSegments);

                target.ExceptWith(singleRouter.TracePoints);
            }

            return true;
        }
    }
}
