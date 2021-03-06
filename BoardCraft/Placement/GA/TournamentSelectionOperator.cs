﻿namespace BoardCraft.Placement.GA
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

        private Board SelectOne(Population p, Random random)
        {
            var n = p.ComponentPlacements.PickRandom(random, _tournamentSize);
            var ordered = n.OrderByDescending(p.GetFitnessFor);

            var factor = 1.0;
            var accumulatedChance = 0.0;
            var pickedNumber = random.NextDouble();
            Board last = null;
            foreach (var P1 in ordered)
            {
                last = P1;
                accumulatedChance += _pressure * factor;
                if (pickedNumber < accumulatedChance)
                {
                    return P1;
                }

                factor *= 1 - _pressure;
            }

            return last;
        }

        public ICollection<Board> Select(Population p)
        {
            var n = p.Count;
            var random = new Random(RandomSeed.Robust());
            var c = new List<Board>(n);

            for (var i = 0; i < n; i++)
            {
                c.Add(SelectOne(p, random));
            }

            return c;
        }
    }
}
