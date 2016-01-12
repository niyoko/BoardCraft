namespace BoardCraft.Routing
{
    using Models;
    using System;
    internal class RouterWorkspace
    {
        internal readonly int[,] _internalData;
        private readonly double _traceWidth;
        private readonly double _minimumSpace;

        public RouterWorkspace(Board board, double traceWidth, double minimumSpace)
        {
            var s = board.GetSize();
            //var wx = Math.Ceiling(s.Width/traceWidth/2);
            //var hx = Math.Ceiling(s.Height/traceWidth/2);            

            var hx = Math.Ceiling(s.Width);
            var wx = Math.Ceiling(s.Height);

            _internalData = new int[(int)(hx), (int)(wx)];
            _traceWidth = traceWidth;
            _minimumSpace = minimumSpace;
        }

        internal int this[IntPoint index]
        {
            get { return _internalData[index.X, index.Y]; }
            set { _internalData[index.X, index.Y] = value; }
        }

        public int Width => _internalData.GetLength(0);
        public int Height => _internalData.GetLength(1);
    }
}