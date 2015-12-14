namespace BoardCraft.Placement.GA
{
    using Models;

    public interface ICrossoverOperator
    {
        void Crossover(ComponentPlacement placement1, ComponentPlacement placement2);
    }
}
