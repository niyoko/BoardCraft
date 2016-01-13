namespace BoardCraft.Routing
{
    using Models;
    using System;
    internal class RouterWorkspace
    {
        internal readonly int[,] _internalData;
        private readonly double _cellSize;

        public Board Board { get; }
        public RouterWorkspace(Board board, double cellSize)
        {
            Board = board;
            var s = board.GetSize();
            _cellSize = cellSize;

            var hx = Math.Ceiling(s.Width/cellSize);
            var wx = Math.Ceiling(s.Height/cellSize);

            _internalData = new int[(int)(hx), (int)(wx)];
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