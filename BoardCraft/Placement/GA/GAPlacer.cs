namespace BoardCraft.Placement.GA
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Models;

    public class GAPlacer : IPlacer
    {
        private readonly IPopulationGenerator _initialPopulationGenerator;
        private readonly IFitnessEvaluator _fitnessEvaluator;
        private readonly IReproductionOperator _reproductionOperator;
        private readonly int _populationSize;

        public GAPlacer(
            int populationSize,
            IPopulationGenerator initialPopulationGenerator,
            IFitnessEvaluator fitnessEvaluator,
            IReproductionOperator reproductionOperator
        )
        {
            _initialPopulationGenerator = initialPopulationGenerator;
            _populationSize = populationSize;
            _fitnessEvaluator = fitnessEvaluator;
            _reproductionOperator = reproductionOperator;
        }

        public event Action<ComponentPlacement> NewGeneration;

        public ComponentPlacement Place(Schematic schema)
        {
            var pop = _initialPopulationGenerator.GeneratePopulation(schema, _populationSize);
            pop.EvaluateFitness(_fitnessEvaluator);
            var sw = new Stopwatch();
            for (var i = 0; i < 5000; i++)
            {   
                sw.Start();
                pop = _reproductionOperator.ProduceNextGeneration(pop);
                var reproTime = sw.ElapsedMilliseconds;
                sw.Restart();
                pop.EvaluateFitness(_fitnessEvaluator);
                var evaTime = sw.ElapsedMilliseconds;
                sw.Reset();

                var h = NewGeneration;

                var pop1 = pop;
                var best2 = pop.Select(x => new { x, f = pop1.GetFitnessFor(x) })
                    .OrderByDescending(x => x.f)
                    .First();

                h?.Invoke(best2.x);
                Debug.WriteLine($"Generation# : {i} Repro : {reproTime} Eva : {evaTime} Fitness : {best2.f}");

            }

            var best = pop.OrderByDescending(pop.GetFitnessFor)
                .First();

            return best;
        }
    }
}