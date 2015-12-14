namespace BoardCraft.Placement.GA
{
    using System.Collections.Generic;
    using Models;

    public interface ISelectionOperator
    {
        ICollection<ComponentPlacement> Select(Population p);
    }
}