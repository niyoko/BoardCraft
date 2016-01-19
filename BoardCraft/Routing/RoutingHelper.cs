namespace BoardCraft.Routing
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    public static class RoutingHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static LPoint OffsetPoint(LPoint point, IntPoint offset)
        {
            var xx = point.Point.X + offset.X;
            var yy = point.Point.Y + offset.Y;

            var p = new LPoint(point.Layer, new IntPoint(xx, yy));
            return p;
        }

        public static ICollection<IntPoint> GetPointsInCircle(IntPoint center, int radius)
        {
            var l = GetHorizontalRads(radius);

            var estimated = (int)(3.15 * radius * (radius + 2));
            var lz = new List<IntPoint>(estimated);

            for (var i = 1; i < l.Length; i++)
            {
                for (var j = 0; j <= l[i]; j++)
                {
                    lz.Add(new IntPoint(center.X + j, center.Y + i));
                    lz.Add(new IntPoint(center.X - j, center.Y - i));
                    lz.Add(new IntPoint(center.X - i, center.Y + j));
                    lz.Add(new IntPoint(center.X + i, center.Y - j));
                }
            }

            return lz;
        }

        private static int[] GetHorizontalRads(int radius)
        {
            var x = radius;
            var y = 0;

            var decisionOver2 = 1 - x;   // Decision criterion divided by 2 evaluated at x=r, y=0
            var l = new int[radius + 1];

            while (y <= x)
            {
                if (x > l[y])
                    l[y] = x;

                if (y > l[x])
                    l[x] = y;

                y++;
                if (decisionOver2 <= 0)
                {
                    decisionOver2 += 2 * y + 1;   // Change in decision criterion for y -> y+1
                }
                else
                {
                    x--;
                    decisionOver2 += 2 * (y - x) + 1;   // Change for y -> y+1, x -> x-1
                }
            }

            return l;
        }
    }
}
