namespace BoardCraft.Placement.GA
{
    using System.Threading.Tasks;
    using MathNet.Numerics.Random;
    using Models;

    public class RandomPopulationGenerator : IPopulationGenerator
    {
        private readonly int _seed;
        private readonly bool _seeded;

        public RandomPopulationGenerator()
        {
            _seeded = false;
        }

        public RandomPopulationGenerator(int seed)
        {
            _seeded = true;
            _seed = seed;
        }

        private int[] GetSubseed(int populationSize)
        {
            var subseed = new int[populationSize];
            if (_seeded)
            {
                var random = new MersenneTwister(_seed, false);
                for (var i = 0; i < populationSize; i++)
                {
                    subseed[i] = random.Next();
                }
            }
            else
            {
                for (var i = 0; i < populationSize; i++)
                {
                    subseed[i] = RandomSeed.Robust();
                }
            }

            return subseed;
        }

        public Population GeneratePopulation(Schematic schema, int populationSize)
        {
            var subseeds = GetSubseed(populationSize);
            var shcs = new ComponentPlacement[populationSize];

            Parallel.For(0, populationSize, i =>
            {
                var placer = new RandomPlacer(subseeds[i]);
                var plc = placer.Place(schema);
                shcs[i] = plc;
            });

            return new Population(shcs);
        }
    }
}
