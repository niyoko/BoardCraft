namespace BoardCraft.Placement.GA
{
    using Models;

    public interface ICrossoverOperator
    {
        void Crossover(Board placement1, Board placement2);
    }
}
