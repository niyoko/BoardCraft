namespace BoardCraft.Placement.GA
{
    using System;
    using System.Diagnostics;
    using Models;

    public class GAPlacer
    {
        private readonly IPopulationGenerator _initialPopulationGenerator;
        private readonly IFitnessEvaluator _fitnessEvaluator;
        private readonly IReproductionOperator _reproductionOperator;
        private readonly Schematic _schematic;
        private readonly int _populationSize;

        public GAPlacer
            (
            Schematic schematic,
            int populationSize,
            IPopulationGenerator initialPopulationGenerator,
            IFitnessEvaluator fitnessEvaluator,
            IReproductionOperator reproductionOperator
        )
        {
            _schematic = schematic;
            _initialPopulationGenerator = initialPopulationGenerator;
            _populationSize = populationSize;
            _fitnessEvaluator = fitnessEvaluator;
            _reproductionOperator = reproductionOperator;
        }

        public int Generation
        {
            get
            {
                if (CurrentPopulation == null)
                {
                    throw new InvalidOperationException("GA not initialized yet");
                }

                return CurrentPopulation.Generation;
            }
        }
        public Population CurrentPopulation { get; private set; }

        public void NextGeneration()
        {
            if (CurrentPopulation == null)
            {
                GenerateInitialPopulation();
            }
            else
            {
                NextPopulationInternal();
            }
        }

        private void GenerateInitialPopulation()
        {
            var pop = _initialPopulationGenerator.GeneratePopulation(_schematic, _populationSize);
            var sw = Stopwatch.StartNew();
            pop.EvaluateFitness(_fitnessEvaluator);
            sw.Stop();
            Debug.WriteLine($"Generation #: {pop.Generation} Fitness Eval Time: {sw.ElapsedMilliseconds}");

            CurrentPopulation = pop;
        }

        private void NextPopulationInternal()
        {
            var pop = _reproductionOperator.ProduceNextGeneration(CurrentPopulation);
            var sw = Stopwatch.StartNew();
            pop.EvaluateFitness(_fitnessEvaluator);
            sw.Stop();
            Debug.WriteLine($"Generation #: {pop.Generation} Fitness Eval Time: {sw.ElapsedMilliseconds}");
            CurrentPopulation = pop;
        }        
    }
}