namespace BoardCraft.Placement.GA
{
    using System;
    using System.Linq;
    using MathNet.Numerics.Statistics;
    using Models;

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

        public double GetDistributionFactor(Bounds[] bounds)
        {
            var w = bounds.Select(x => x.Right).Max();
            var h = bounds.Select(x => x.Top).Max();

            var hx = w / 2;
            var hy = h / 2;

            var bnds = new[]
            {
                new Bounds(hy, hx, 0, 0),
                new Bounds(hy, w,0,hx),
                new Bounds(h,hx,hy,0),
                new Bounds(h,w,hx,hy),    
            };
            var area = new double[4];
            foreach (var b in bounds)
            {
                for (var i = 0; i < bnds.Length; i++)
                {
                    var ov = GetOverlap(b, bnds[i]);
                    area[i] += ov;
                }
            }

            return area.StandardDeviation();
        }

        public double EvaluateFitness(Board board)
        {
            board.CalculateBounds();
            var dist = board.CalculatePinDistances();
            var dd = dist.Values.Count == 0 ? 0 : 100*dist.Values.Select(x => x.Max).Average();

            var b = new Bounds[board.Schema.Components.Count];
            int i = 0;
            foreach (var component in board.Schema.Components)
            {
                var bx = board.GetBounds(component);
                b[i++] = bx;
            }

            var f1 = GetOverlappedArea(b);
            var sqEqOl = Math.Sqrt(f1);

            var d = 10*AverageDistance(b);

            return (1 / (sqEqOl + 1)) + (1 / (d + 1)) + (1/(dd+1));
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
