namespace BoardCraft.Placement.GA
{
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

            GenerationNumber = 0;
        }

        public int GenerationNumber { get; private set; }
        public Population CurrentPopulation { get; private set; }

        public void NextGeneration()
        {
            if (GenerationNumber == 0)
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
            pop.EvaluateFitness(_fitnessEvaluator);

            CurrentPopulation = pop;
            GenerationNumber = 1;
        }

        private void NextPopulationInternal()
        {
            var pop = _reproductionOperator.ProduceNextGeneration(CurrentPopulation);
            pop.EvaluateFitness(_fitnessEvaluator);

            CurrentPopulation = pop;
            GenerationNumber++;
        }        
    }
}