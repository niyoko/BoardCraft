namespace BoardCraft.Placement.GA
{
    using System;
    using System.Linq;
    using Models;
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

        private struct Size
        {
            public Size(double width, double height)
            {
                Width = width;
                Height = height;
            }

            public double Width;
            public double Height;
        }

        private Bounds[] CalculateBounds(ComponentPlacement placement)
        {
            var componentList = placement.Schema.Components.ToList();
            var count = componentList.Count;
            var d = new Bounds[count];

            for (var i = 0; i < count; i++)
            {
                var c = componentList[i];
                var p = placement.GetComponentPlacement(c);
                d[i] = GetRealBound(p, c.Package);
            }

            return d;
        }

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
            }

            return sum;
        }

        public static double AverageDistance(ComponentPlacement board)
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

        private static readonly int[][] PointTransformer = new[]
        {
            new [] {1, 0, 0, 1}, //up
            new [] {0, -1, 1, 0}, //left
            new [] {-1, 0, 0, -1},
            new [] {0, 1, -1, 0}
        };

        private double TidynessFitness(ComponentPlacement board, Size size)
        {
            var pinCount = board.Schema.Components.Select(x => x.Package.Pins.Count).Sum();
            var points = new Point[pinCount];

            var i = 0;
            foreach (var c in board.Schema.Components)
            {
                var p = board.GetComponentPlacement(c);
                var t = PointTransformer[(int)p.Orientation];

                foreach (var pin in c.Package.Pins)
                {
                    var pos = pin.Position;
                    var x = t[0] * pos.X + t[1] * pos.Y;
                    var y = t[2] * pos.X + t[3] * pos.Y;

                    points[i++] = new Point(x, y);
                }
            }
        }

        private Size GetBoardSize(Bounds[] bounds)
        {
            double w = 0, h = 0;
            for (var i = 0; i < bounds.Length; i++)
            {
                var b = bounds[i];
                if (b.Right > w)
                {
                    w = b.Right;
                }

                if (b.Top > h)
                {
                    h = b.Top;
                }
            }

            return new Size(w, h);
        }

        public double EvaluateFitness(ComponentPlacement board)
        {
            var b = CalculateBounds(board);
            var s = GetBoardSize(b);

            var f1 = GetOverlappedArea(b);
            var f2 = AverageDistance(board);
            var f3 = TidynessFitness(board, s);

            return (1 / (Math.Sqrt(f1) + 1)) + (1 / (f2 + 1));
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
