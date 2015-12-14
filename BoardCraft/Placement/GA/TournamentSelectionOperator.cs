namespace BoardCraft.Placement.GA
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Helpers;
    using MathNet.Numerics.Random;
    using Models;

    public class TournamentSelectionOperator : ISelectionOperator
    {
        private readonly int _tournamentSize;
        private readonly double _pressure;

        public TournamentSelectionOperator(int tournamentSize, double selectionPressure)
        {
            _tournamentSize = tournamentSize;
            _pressure = selectionPressure;
        }

        private ComponentPlacement SelectOne(Population p, Random random)
        {
            var n = p.ComponentPlacements.PickRandom(random, _tournamentSize);
            var ordered = n.OrderByDescending(p.GetFitnessFor);

            var factor = 1.0;
            var accumulatedChance = 0.0;
            var pickedNumber = random.NextDouble();
            ComponentPlacement last = null;
            foreach (var p1 in ordered)
            {
                last = p1;
                accumulatedChance += _pressure * factor;
                if (pickedNumber < accumulatedChance)
                {
                    return p1;
                }

                factor *= 1 - _pressure;
            }

            return last;
        }

        public ICollection<ComponentPlacement> Select(Population p)
        {
            var n = p.Count;
            var random = new Random(RandomSeed.Robust());
            var c = new List<ComponentPlacement>(n);

            for (var i = 0; i < n; i++)
            {
                c.Add(SelectOne(p, random));
            }

            return c;
        }
    }
}
