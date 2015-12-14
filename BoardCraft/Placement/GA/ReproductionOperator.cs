namespace BoardCraft.Placement.GA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using MathNet.Numerics.Random;
    using Models;

    public class ReproductionOperator : IReproductionOperator
    {
        private readonly ISelectionOperator _selectionOperator;

        private readonly double _crossoverRate;
        private readonly ICrossoverOperator _crossoverOperator;

        private readonly double _mutationRate;
        private readonly IMutationOperator _mutationOperator;

        public ReproductionOperator(
            ISelectionOperator selector, 

            double crossoverRate,
            ICrossoverOperator crossover, 

            double mutationRate,
            IMutationOperator mutation
        )
        {
            _selectionOperator = selector;

            _crossoverRate = crossoverRate;
            _crossoverOperator = crossover;

            _mutationRate = mutationRate;
            _mutationOperator = mutation;
        }

        public Population ProduceNextGeneration(Population parents)
        {
            var random = new Random(RandomSeed.Robust());

            //select using selection operator and clone it
            var offspring = _selectionOperator.Select(parents)
                .Select(x => x.Clone())
                .ToList();
            
            var crossed = offspring
                .Select(c => new {c, r = random.NextDouble()})
                .Where(x => x.r < _crossoverRate)
                .OrderBy(x=>x.r)
                .Select(x=>x.c)
                .ToList();
           
            if (crossed.Count%2 == 1)
            {
                crossed.RemoveAt(crossed.Count - 1);
            }

            for (var i = 0; i < crossed.Count/2; i++)
            {
                var i1 = 2*i;
                var i2 = 2*i + 1;

                _crossoverOperator.Crossover(crossed[i1], crossed[i2]);
            }

            var mutated = offspring
                .Select(c => new {c, r = random.NextDouble()})
                .Where(x => x.r < _mutationRate)
                .Select(x => x.c)
                .ToList();

            foreach (var m in mutated)
            {
                _mutationOperator.Mutate(m);
            }

            return new Population(offspring);
        }
    }
}
