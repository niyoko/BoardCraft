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

            var hx = Math.Ceiling((s.Width+500)/cellSize);
            var wx = Math.Ceiling((s.Height+500)/cellSize);

            _internalData = new int[(int)(hx), (int)(wx)];
        }

        public bool IsPointValid(IntPoint index)
        {
            var valid = true;
            valid = valid && index.X >= 0;
            valid = valid && index.X < Width;
            valid = valid && index.Y >= 0;
            valid = valid && index.Y < Height;
            return valid;
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