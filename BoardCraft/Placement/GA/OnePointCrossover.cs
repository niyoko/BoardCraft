using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BoardCraft.Models;
using MathNet.Numerics.Random;

namespace BoardCraft.Placement.GA
{
    public class OnePointCrossover : ICrossoverOperator
    {
        public void Crossover(ComponentPlacement placement1, ComponentPlacement placement2)
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

            var cl = sch.Components.ToList();
            var bp = random.Next(cl.Count);

            for(var i = 0; i<= bp; i++)
            {
                var comp = cl[i];
                var p1 = placement1.GetComponentPlacement(comp);
                var p2 = placement2.GetComponentPlacement(comp);

                placement1.SetComponentPlacement(comp, p2);
                placement2.SetComponentPlacement(comp, p1);
            }
        }
    }
}
