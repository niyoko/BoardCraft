using System;

namespace BoardCraft.Placement.GA
{
    using Helpers;
    using MathNet.Numerics.Random;
    using Models;

    public class CrossoverOperator : ICrossoverOperator
    {
        private readonly int _crossedComponentMin;
        private readonly int _crossedComponentMax;

        public CrossoverOperator(int crossedComponentMin, int crossedComponentMax)
        {
            _crossedComponentMin = crossedComponentMin;
            _crossedComponentMax = crossedComponentMax;
        }

        public void Crossover(Board placement1, Board placement2)
        {
            if (placement1 == null)
            {
                throw new ArgumentNullException(nameof(placement1));
            }

            if (placement2 == null)
            {
                throw new ArgumentNullException(nameof(placement2));
            }

            var sch = placement1.Schema;
            if (placement2.Schema != sch)
            {
                throw new ArgumentException("Placements does not refer to same schema");
            }

            var random = new Random(RandomSeed.Robust());        
            

            var cCount = _crossedComponentMin == _crossedComponentMax 
                ? _crossedComponentMin 
                : random.Next(_crossedComponentMin, _crossedComponentMax + 1);

            var comps = sch.Components.PickRandom(random, cCount);
            foreach (var comp in comps)
            {
                var p1 = placement1.GetComponentPlacement(comp);
                var p2 = placement2.GetComponentPlacement(comp);
                

                placement1.SetComponentPlacement(comp, p2);
                placement2.SetComponentPlacement(comp, p1);
            }
        }
    }
}
