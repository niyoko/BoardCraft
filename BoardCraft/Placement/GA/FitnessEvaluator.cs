namespace BoardCraft.Placement.GA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Models;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using Drawing;

    public class FitnessEvaluator : IFitnessEvaluator
    {
        private struct Bounds
        {
            public Bounds(double top, double right, double bottom, double left)
            {
                Top = top;
                Right = right;
                Bottom = bottom;
                Left = left;
            }

            public double Top;
            public double Right;
            public double Bottom;
            public double Left;
        }

        public double OverlapFitness(ComponentPlacement board)
        {
            var componentList = board.Schema.Components.ToList();
            var count = componentList.Count;
            var d = new Bounds[count];

            for (var i = 0; i < count; i++)
            {
                var c = componentList[i];
                var p = board.GetComponentPlacement(c);
                d[i] = GetRealBound(p, c.Package);
            }

            var sum = 0.0;
            for (var i = 0; i < count; i++)
            {
                for (var j = i + 1; j < count; j++)
                {
                    sum += GetOverlap(d[i], d[j]);
                }
            }

            var sumx = Math.Sqrt(sum) + 1;
            return 1 / sumx;
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
        
        private static Bounds GetRealBound(PlacementInfo metadata, Package p)
        {
            double left = 0, top = 0, right = 0, bottom = 0;
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

            return new Bounds(top, right, bottom, left);
        }

        private static double GetOverlap(Bounds b1, Bounds b2)
        {
            var rig = b1.Right < b2.Right ? b1.Right : b2.Right;
            var lef = b1.Left > b2.Left ? b1.Left : b2.Left;
            var top = b1.Top < b2.Top ? b1.Top : b2.Top;
            var bot = b1.Bottom > b2.Bottom ? b1.Bottom : b2.Bottom;

            if (lef >= rig || bot >= top)
                return 0.0;

            return (rig - lef)*(top - bot);
        }
    }
}
