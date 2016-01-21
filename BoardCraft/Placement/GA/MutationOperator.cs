namespace BoardCraft.Placement.GA
{
    using System;
    using Drawing;
    using Helpers;
    using MathNet.Numerics.Random;
    using Models;

    public class MutationOperator : IMutationOperator
    {
  

        public void Mutate(Board placement)
        {
            var random = new Random(RandomSeed.Robust());

            var mutated = placement.Schema.Components.PickRandom(random);
            var info = placement.GetComponentPlacement(mutated);

            var mutatedElement = random.Next(4);

            if (mutatedElement == 0)
            {
                var o = random.Next(3);
                if (o == 0)
                {
                    o = -1;
                }

                var newO = (Orientation) (((int) info.Orientation + o + 4)%4);
                info = new PlacementInfo(info.Position, newO);
                placement.SetComponentPlacement(mutated, info);
            }
            else
            {
                var shx = 0.0;
                var shy = 0.0;

                var res = random.Next(12);
                if (res == 8)
                    res = 4;
                if (res == 9 || res == 10)
                    res = 5;
                if (res == 11)
                    res = 6;

                var P1 = random.NextDouble();

                var amount = 10;
                if (P1 < 0.05)
                {
                    amount = 200;
                }
                else if (P1 < 0.1)
                {
                    amount = 100;
                }
                else if (P1 < 0.2)
                {
                    amount = 50;
                }

                if (res == 1 || res == 2 || res == 3)
                {
                    shx = 1 * amount;
                }

                if (res == 5 || res == 6 || res == 7)
                {
                    shx = -1 * amount;
                }

                if (res == 7 || res == 0 || res == 1)
                {
                    shy = 1 * amount;
                }

                if (res == 3 || res == 4 || res == 5)
                {
                    shy = -1 * amount;
                }

                info = new PlacementInfo(new Point(info.Position.X + shx, info.Position.Y + shy), info.Orientation);
                placement.SetComponentPlacement(mutated, info);
            }
        }
    }

    public interface IMutationOperator
    {
        void Mutate(Board population);
    }
}
