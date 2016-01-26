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

            /*
            var yy = board.Schema.Components.SelectMany(x => x.Pins).ToList();
            var z = new List<Point>(yy.Count);
            z.AddRange(yy.Select(board.GetPinLocation));

            //x axis
            var cx = new HashSet<int>();
            var cy = new HashSet<int>();

            for (var p = 0; p < z.Count; p++)
            {
                cx.Add((int)(z[p].X/10.0));
                cy.Add((int)(z[p].Y/10.0));
            }

            Debug.WriteLine(cx.Count);
            var c = (2 * z.Count) / (cx.Count + cy.Count);*/
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
