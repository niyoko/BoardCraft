using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Routing
{
    using System.Collections.ObjectModel;

    internal class AbstractTraceSegment
    {
        private List<IntPoint> _internalNodes;
        public IList<IntPoint> Nodes { get; } 
        public WorkspaceLayer Layer { get; }

        public AbstractTraceSegment(WorkspaceLayer layer, IEnumerable<IntPoint> nodes)
        {
            Layer = layer;
            
            _internalNodes = new List<IntPoint>(nodes);
            Nodes = new ReadOnlyCollection<IntPoint>(_internalNodes);
        }
    }
}
