namespace BoardCraft.Placement
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Drawing;
    using MathNet.Numerics.Random;
    using Models;

    public class RandomPlacer
    {
        private const double GoldenRatio = 1.61803399;

        public RandomPlacer() : this(Environment.TickCount)
        {
        }

        public RandomPlacer(int randomSeed)
            : this(randomSeed, 5, GoldenRatio)
        {
        }

        public RandomPlacer(double spaceFactor, double aspectRatio)
            : this(Environment.TickCount, spaceFactor, aspectRatio)
        {
        }

        public RandomPlacer(int randomSeed, double spaceFactor, double aspectRatio)
        {
            RandomSeed = randomSeed;
            SpaceFactor = spaceFactor;
            AspectRatio = aspectRatio;
        }

        private int RandomSeed { get; }

        private double SpaceFactor { get; }

        private double AspectRatio { get; }

        public Board Place(Schematic schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            var random = new MersenneTwister(RandomSeed);
            var totalArea = schema.Components.Sum(x => x.Package.Boundaries.Area);

            var boardArea = SpaceFactor * totalArea;
            var h = Math.Sqrt(boardArea / AspectRatio);
            var w = Math.Sqrt(boardArea * AspectRatio);

            var hi = (int) (h/10) + 1;
            var wi = (int) (w/10) + 1;

            var p = new Board(schema);

            foreach (var component in schema.Components)
            {
                var x = 10*random.Next(wi);
                var y = 10*random.Next(hi);
                var or = random.Next(4);

                var pos = new Point(x, y);
                var ori = (Orientation)or;

                p.SetComponentPlacement(component, pos, ori);
            }

            return p;
        }
    }
}