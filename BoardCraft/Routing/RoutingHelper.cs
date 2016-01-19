namespace BoardCraft.Routing
{
    using System.Collections.Generic;

    public static class RoutingHelper
    {
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
            int x = radius;
            int y = 0;

            int decisionOver2 = 1 - x;   // Decision criterion divided by 2 evaluated at x=r, y=0
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
