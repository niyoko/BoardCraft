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

    public class Population : IEnumerable<ComponentPlacement>
    {
        private readonly ConcurrentDictionary<ComponentPlacement, double> _fitness;        
        private bool _fitnessCalculated;
        private ComponentPlacement _bestPlacement;

        public Population(int generation, IEnumerable<ComponentPlacement> componentPlacements)
        {
            var list = new List<ComponentPlacement>(componentPlacements);
            ComponentPlacements = new ReadOnlyCollection<ComponentPlacement>(list);
            _fitness = new ConcurrentDictionary<ComponentPlacement, double>();
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

        public ICollection<ComponentPlacement> ComponentPlacements { get; }
        
        public int Count => ComponentPlacements.Count;

        public double GetFitnessFor(ComponentPlacement placement)
        {
            EnsureFitnessEvaluated();
            return _fitness[placement];
        }

        public IEnumerator<ComponentPlacement> GetEnumerator()
        {
            return ComponentPlacements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public ComponentPlacement BestPlacement
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
