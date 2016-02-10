namespace BoardCraft.Placement.GA
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Drawing;
    using Models;

    public class FitnessEvaluator : IFitnessEvaluator
    {
        private static double GetOverlappedArea(Bounds[] bounds)
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

        public static double AverageDistance(Bounds[] bounds)
        {
            var sumC =
                from c
                    in bounds
                let xp = c.Right*c.Right
                let yp = c.Top*c.Top
                select Math.Sqrt(xp + yp);

            var sum = sumC.Sum();
            var cnt = bounds.Length;
            return cnt == 0 ? 0 : sum / cnt;
        }
        
        public double EvaluateFitness(Board board)
        {
            board.CalculateBounds();
            var dist = board.CalculatePinDistances();
            var dd = dist.Values.Count == 0 ? 0 : 100*dist.Values.Select(x => x.Average).Average();

            var b = new Bounds[board.Schema.Components.Count];
            var i = 0;
            foreach (var component in board.Schema.Components)
            {
                var bx = board.GetBounds(component);
                b[i++] = bx;
            }

            var f1 = GetOverlappedArea(b);
            var sqEqOl = Math.Sqrt(f1);

            var d = 10*AverageDistance(b);
            
            return (1 / (sqEqOl + 1)) + (1 / (d + 1)) + (1/(dd+1)) + (1/(5*GetHighPowerAverageDistance(board)+1));
        }

        private static double GetHighPowerAverageDistance(Board b)
        {
            var s = b.GetSize();
            var hp = b.Schema.Components.Where(x => x.IsHighPower).ToList();
            var hpc = hp.Count;
            if (hpc == 0)
                return 0;

            var sum = 0.0;
            foreach (var c in hp)
            {
                var p = b.GetComponentPlacement(c).Position;
                var le = p.X;
                var bo = p.Y;
                var to = s.Height - p.Y;
                var ri = s.Width - p.X;

                var d = le;
                d = bo < d ? bo : d;
                d = to < d ? to : d;
                d = ri < d ? ri : d;
                if (d < 0)
                    d = 0;
                sum += d;
            }

            return sum/hpc;

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
            var rig = (b1.Right+20) < (b2.Right+20) ? (b1.Right+20) : (b2.Right+20);
            var lef = (b1.Left-20) > (b2.Left-20) ? (b1.Left-20) : (b2.Left-20);
            var top = (b1.Top+20) < (b2.Top+20) ? (b1.Top+20) : (b2.Top+20);
            var bot = (b1.Bottom-20) > (b2.Bottom-20) ? (b1.Bottom-20) : (b2.Bottom-20);

            if (lef >= rig || bot >= top)
                return 0.0;

            return (rig - lef) * (top - bot);
        }
    }
}
