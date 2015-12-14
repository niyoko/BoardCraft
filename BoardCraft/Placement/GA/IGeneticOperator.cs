using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BoardCraft.Placement.GA
{
    using Models;

    interface IGeneticOperator
    {
        Population DoOperation(ICollection<Schematic> schema);
    }
}
