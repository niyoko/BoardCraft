using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Placement.GA
{
    public interface IReproductionOperator
    {
        Population ProduceNextGeneration(Population parents);
    }
}
