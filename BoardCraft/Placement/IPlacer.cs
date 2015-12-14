namespace BoardCraft.Placement
{
    using System.Threading.Tasks;
    using Models;

    public interface IPlacer
    {
        ComponentPlacement Place(Schematic schema);
    }
}
