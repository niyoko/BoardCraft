namespace BoardCraft.Placement.GA
{
    using System.Collections.Generic;
    using Models;

    public interface ISelectionOperator
    {
        ICollection<Board> Select(Population p);
    }
}