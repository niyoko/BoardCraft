namespace BoardCraft.Placement.GA
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Models;

    public class Population : IEnumerable<Board>
    {
        private readonly ConcurrentDictionary<Board, double> _fitness;        
        private bool _fitnessCalculated;
        private Board _bestPlacement;

        public Population(int generation, IEnumerable<Board> componentPlacements)
        {
            var list = new List<Board>(componentPlacements);
            ComponentPlacements = new ReadOnlyCollection<Board>(list);
            _fitness = new ConcurrentDictionary<Board, double>();
            _fitnessCalculated = false;

            Generation = generation;
        }

        public int Generation { get; }

        private void EnsureFitnessEvaluated()
        {
            if (!_fitnessCalculated)
            {
                throw new InvalidOperationException("Fitness not evaluated yet");
            }
        }

        public void EvaluateFitness(IFitnessEvaluator evaluator)
        {
            _fitnessCalculated = false;
            _fitness.Clear();
            _bestPlacement = null;

            Parallel.ForEach(ComponentPlacements, p =>
            {
                var fitness = evaluator.EvaluateFitness(p);
                var addSuccess = _fitness.TryAdd(p, fitness);
                if (!addSuccess)
                {
                    throw new ArgumentException("Component placement already exists in fitness values");
                }
            });

            _fitnessCalculated = true;
        }

        public ICollection<Board> ComponentPlacements { get; }
        
        public int Count => ComponentPlacements.Count;

        public double GetFitnessFor(Board placement)
        {
            EnsureFitnessEvaluated();
            return _fitness[placement];
        }

        public IEnumerator<Board> GetEnumerator()
        {
            return ComponentPlacements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Board BestPlacement
        {
            get
            {
                if (!_fitnessCalculated)
                {
                    throw new InvalidOperationException("Fitness not calculated yet");
                }

                if (_bestPlacement == null)
                {
                    _bestPlacement = _fitness
                        .OrderByDescending(x => x.Value)
                        .Select(x => x.Key)
                        .First();
                }

                return _bestPlacement;
            }
        }
    }
}
