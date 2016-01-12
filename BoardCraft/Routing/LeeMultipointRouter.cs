using BoardCraft.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace BoardCraft.Routing
{
    internal class LeeMultipointRouter
    {
        private readonly ISet<IntPoint> _nodes;
        private ISet<IntPoint> _nodesWrapper;

        internal RouterWorkspace Workspace { get; }

        public ISet<IntPoint> Trace { get; }

        public LeeMultipointRouter(RouterWorkspace workspace, ISet<IntPoint> nodes)
        {
            Workspace = workspace;
            _nodes = nodes;

            Trace = new HashSet<IntPoint>();
        }

        public ISet<IntPoint> Nodes
        {
            get
            {
                if (_nodesWrapper == null)
                {
                    _nodesWrapper = new ReadOnlySet<IntPoint>(_nodes);
                }

                return _nodesWrapper;
            }
        }

        public bool Route()
        {
            var rp = Nodes.ToArray()[0];
            Trace.Add(rp);

            while (!IsFinished())
            {
                var target = new HashSet<IntPoint>(Nodes);
                target.ExceptWith(Trace);
                var singleRouter = new LeeRouter(Workspace, Trace, target);
                var rResult = singleRouter.Route();
                if (!rResult)
                {
                    return false;
                }

                Trace.UnionWith(singleRouter.Track);
            }

            return true;
        }

        private bool IsFinished()
        {
            return Nodes.All(x => Trace.Contains(x));
        }
    }
}
