namespace BoardCraft.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using NLog;

    public class Router
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private double TraceWidth { get; }
        private double MinimumSpacing { get; }

        public Router(double traceWidth, double minimumSpacing)
        {
            TraceWidth = traceWidth;
            MinimumSpacing = minimumSpacing;
        }

        public void Route(Board board)
        {
            board.CalculatePinLocations();
            var workspace = new RouterWorkspace(board, TraceWidth, MinimumSpacing);
            var z = board.Schema.Connections.First();
            var pts = new HashSet<IntPoint>();
            foreach (var z1 in z.Pins)
            {
                var pos = board.GetPinLocation(z1.Component, z1.Pin);
                pts.Add(new IntPoint((int) Math.Round(pos.X), (int) Math.Round(pos.Y)));                
            }

            
            var r = new LeeMultipointRouter(workspace, pts);
            r.Route();
            board._traces.Add(r.TraceNodes);
        }
    }
}