namespace BoardCraft.Placement.GA
{
    using System;
    using System.Linq;
    using Models;
    using System.Collections.Generic;

    public class FitnessEvaluator : IFitnessEvaluator
    {
        private double GetOverlappedArea(Bounds[] bounds)
        {
            var count = bounds.Length;
            var d = bounds;

            var sum = 0.0;
            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    sum += GetOverlap(d[i], d[j]);
                }

                sum += GetOutOfRangeArea(d[i]);
            }

            return sum;
        }

        public static double AverageDistance(Board board)
        {
            var sum = 0.0;

            foreach (var c in board.Schema.Components)
            {
                var p = board.GetComponentPlacement(c);
                var pos = p.Position;

                var xsq = pos.X * pos.X;
                var ysq = pos.Y * pos.Y;

                var dist = Math.Sqrt(xsq + ysq);
                sum += dist;
            }

            var cnt = board.Schema.Components.Count;
            return cnt == 0 ? 0 : sum / cnt;
        }

        public double EvaluateFitness(Board board)
        {
            //var b = board.GetBounds();
            //var s = board.GetSize();

            var b = new Bounds[board.Schema.Components.Count];
            int i = 0;
            foreach (var component in board.Schema.Components)
            {
                var bx = board.GetBounds(component);
                b[i++] = bx;
            }

            var f1 = GetOverlappedArea(b);
            var f2 = AverageDistance(board);
            //var f3 = TidynessFitness(board, s);

            return (1/(Math.Sqrt(f1) + 1)) + (1/(f2 + 1));// + f3;
        }

        private static double GetOutOfRangeArea(Bounds b)
        {
            if (b.Left >= 0 && b.Bottom >= 0)
            {
                return 0;
            }

            var w = b.Right - b.Left;
            var h = b.Top - b.Bottom;
            var wx = b.Right >= 0 ? -b.Left : w;
            var hx = b.Top >= 0 ? -b.Bottom : h;
            if (wx < 0) wx = 0;
            if (hx < 0) hx = 0;

            return (wx*h) + (hx*w) - (wx*hx);
        }

        private static double GetOverlap(Bounds b1, Bounds b2)
        {
            var rig = b1.Right < b2.Right ? b1.Right : b2.Right;
            var lef = b1.Left > b2.Left ? b1.Left : b2.Left;
            var top = b1.Top < b2.Top ? b1.Top : b2.Top;
            var bot = b1.Bottom > b2.Bottom ? b1.Bottom : b2.Bottom;

            if (lef >= rig || bot >= top)
                return 0.0;

            return (rig - lef) * (top - bot);
        }
    }
}
