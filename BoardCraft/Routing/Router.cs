namespace BoardCraft.Routing
{
    using System.Diagnostics;
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
            var workspace = new RouterWorkspace(board, TraceWidth, MinimumSpacing);
            var c = board.Schema.Connections.Count;
            var z = board.Schema.Connections.First();



            //_logger.Debug($" -- {c}");
            /*for (var i = 0; i < c; i++)
            {
                var mpRouter = 
            }*/
        }
    }
}
