﻿namespace BoardCraft.Routing
{
    using System.Diagnostics;
    using BoardCraft.Helpers;
    using System.Collections.Generic;
    using System.Linq;
    using NLog.Targets;

    internal class LeeMultipointRouter
    {
        private readonly ISet<LPoint> _pins;
        private ISet<LPoint> _pinsWrapper;

        internal RouterWorkspace Workspace { get; }

        public ISet<LPoint> Trace { get; }
        public ICollection<IList<LPoint>> TraceNodes { get; } 

        public LeeMultipointRouter(RouterWorkspace workspace, ISet<IntPoint> pins)
        {
            Workspace = workspace;
            _pins = new HashSet<LPoint>(pins.Select(x=> new LPoint(WorkspaceLayer.BottomLayer, x)));

            Trace = new HashSet<LPoint>();
            TraceNodes = new List<IList<LPoint>>();
        }

        public ISet<LPoint> Pins
        {
            get
            {
                if (_pinsWrapper == null)
                {
                    _pinsWrapper = new ReadOnlySet<LPoint>(_pins);
                }

                return _pinsWrapper;
            }
        }


        internal ISet<LPoint> _target; 
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
                    _target = target;
                    return false;
                }

                Trace.UnionWith(singleRouter.Track);
                TraceNodes.Add(singleRouter.TrackNodes);
                target.ExceptWith(Trace);
            }

            return true;
        }
    }
}
