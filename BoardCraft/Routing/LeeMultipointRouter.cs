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

        public ISet<IntPoint> Trace { get; }
        public ICollection<IList<IntPoint>> TraceNodes { get; } 

        public LeeMultipointRouter(RouterWorkspace workspace, ISet<IntPoint> pins)
        {
            Workspace = workspace;
            _pins = pins;

            Trace = new HashSet<IntPoint>();
            TraceNodes = new List<IList<IntPoint>>();
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
            Trace.Add(rp);

            while (!IsFinished())
            {
                var target = new HashSet<IntPoint>(Pins);
                target.ExceptWith(Trace);
                var singleRouter = new LeeRouter(Workspace, Trace, target);
                var rResult = singleRouter.Route();
                if (!rResult)
                {
                    return false;
                }

                Debug.WriteLine($"Track cnt {singleRouter.Track.Count}");
                Trace.UnionWith(singleRouter.Track);
                TraceNodes.Add(singleRouter.TrackNodes);
            }

            return true;
        }

        private bool IsFinished()
        {
            return Pins.All(x => Trace.Contains(x));
        }
    }
}
