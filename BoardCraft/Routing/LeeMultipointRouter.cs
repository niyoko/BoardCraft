namespace BoardCraft.Routing
{
    using Helpers;
    using System.Collections.Generic;
    using System.Linq;

    internal class LeeMultipointRouter
    {
        private readonly ISet<LPoint> _pins;
        private ISet<LPoint> _pinsWrapper;

        internal RouterWorkspace Workspace { get; }

        public ISet<LPoint> Trace { get; }
        public ICollection<IList<LPoint>> TraceNodes { get; } 
        public ISet<IntPoint> Vias { get; } 

        public LeeMultipointRouter(RouterWorkspace workspace, ISet<IntPoint> pins)
        {
            Workspace = workspace;
            _pins = new HashSet<LPoint>(pins.Select(x=> new LPoint(WorkspaceLayer.BottomLayer, x)));

            Trace = new HashSet<LPoint>();
            TraceNodes = new List<IList<LPoint>>();
            Vias = new HashSet<IntPoint>();
        }

        public ISet<LPoint> Pins => _pinsWrapper ?? (_pinsWrapper = new ReadOnlySet<LPoint>(_pins));

        public bool Route()
        {
            var target = new HashSet<LPoint>(Pins);
            var rp = Pins.ToArray()[0];
            Trace.Add(rp);
            target.ExceptWith(Trace);

            while (target.Count > 0)
            {
                var singleRouter = new LeeRouter(Workspace, Trace, target);
                var rResult = singleRouter.Route();
                if (!rResult)
                {
                    return false;
                }

                Trace.UnionWith(singleRouter.Track);
                Vias.UnionWith(singleRouter.Vias);
                TraceNodes.Add(singleRouter.TrackNodes);
                target.ExceptWith(Trace);
            }

            return true;
        }
    }
}
