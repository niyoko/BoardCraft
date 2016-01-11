using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Placement.GA
{
    using Models;

    public interface IFitnessEvaluator
    {
        double EvaluateFitness(Board schema);
    }
}
