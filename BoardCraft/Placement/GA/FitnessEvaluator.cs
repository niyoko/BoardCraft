namespace BoardCraft.Placement.GA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using System.Collections.Concurrent;

    public class FitnessEvaluator : IFitnessEvaluator
    {
        private ConcurrentDictionary<Package, double> _maxBounds = new ConcurrentDictionary<Package, double>();

        public double OverlapFitness(ComponentPlacement board)
        {
            var componentList = board.Schema.Components.ToList();
            var count = componentList.Count;

            var sum = 0.0;
            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    var ci = componentList[i];
                    var cj = componentList[j];

                    var thi = GetThresold(ci.Package);
                    var thj = GetThresold(cj.Package);
                    var tht = thi + thj;

                    var pi = board.GetComponentPlacement(ci).Position;
                    var pj = board.GetComponentPlacement(cj).Position;

                    var dx = pi.X - pj.X;
                    var dy = pi.Y - pj.Y;

                    if(dx < tht && dx > -tht && dy < tht && dy > -tht)
                    {
                        sum += GetOverlap(board, componentList, i, j);
                    }
                }
            }

            var sumx = Math.Sqrt(sum) + 1;
            return 1 / sumx;
        }

        private double GetThresold(Package package)
        {
            double result;            
            if (!_maxBounds.TryGetValue(package, out result))
            {
                var b = package.Boundaries;
                var to = b.Top;
                var ri = b.Right;
                var bo = -b.Bottom;
                var le = -b.Left;

                var r = 0.0;
                if (to > r) r = bo;
                if (ri > r) r = ri;
                if (bo > r) r = bo;
                if (le > r) r = le;

                _maxBounds[package] = r;
                return r;
            }

            return result;          
        }

        public static double SizeFitness(ComponentPlacement board)
        {
            var compDists = (
                from component in board.Schema.Components
                select board.GetComponentPlacement(component)
                into m
                select m.Position.X * m.Position.X
                       + m.Position.Y * m.Position.Y
                into m1
                select Math.Sqrt(m1)
            );

            var sum = compDists.Sum();

            return board.Schema.Components.Count / (sum + 1);
        }

        public double EvaluateFitness(ComponentPlacement board)
        {
            var ovlp = OverlapFitness(board);
            var sz = SizeFitness(board);

            return ovlp + sz;
        }

        private static void GetRealBound(PlacementInfo metadata, Package p, out double left, out double top, out double right, out double bottom)
        {
            left = 0;
            top = 0;
            bottom = 0;
            right = 0;
            var package = p.Boundaries;
            switch (metadata.Orientation)
            {
                case Orientation.Up:
                    left = package.Left;
                    top = package.Top;
                    right = package.Right;
                    bottom = package.Bottom;
                    break;
                case Orientation.Left:
                    left = -package.Top;
                    top = package.Right;
                    right = -package.Bottom;
                    bottom = package.Left;
                    break;
                case Orientation.Down:
                    left = -package.Right;
                    top = -package.Bottom;
                    right = -package.Left;
                    bottom = -package.Top;
                    break;
                case Orientation.Right:
                    left = package.Bottom;
                    top = -package.Left;
                    right = package.Top;
                    bottom = -package.Right;
                    break;
            }

            left = metadata.Position.X + left;
            top = metadata.Position.Y + top;
            right = metadata.Position.X + right;
            bottom = metadata.Position.Y + bottom;
        }

        private static double GetOverlap(ComponentPlacement board, IList<Component> list, int i, int j)
        {
            var c1 = list[i];
            var c2 = list[j];

            var gene1 = board.GetComponentPlacement(c1);
            var gene2 = board.GetComponentPlacement(c2);

            double l1, t1, r1, b1;
            GetRealBound(gene1, c1.Package, out l1, out t1, out r1, out b1);

            double l2, t2, r2, b2;
            GetRealBound(gene2, c2.Package, out l2, out t2, out r2, out b2);

            var overlapX = Math.Max(0, Math.Min(r1, r2) - Math.Max(l1, l2));
            var overlapY = Math.Max(0, Math.Min(t1, t2) - Math.Max(b1, b2));

            var overlap = overlapX * overlapY;

            return overlap;
        }
    }
}
