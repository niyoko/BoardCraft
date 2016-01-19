namespace BoardCraft.Input
{
    using Models;

    /// <summary>
    ///     Provide abstraction for component repository
    /// </summary>
    public interface IComponentRepository
    {
        Package GetPackage(string name);
    }
}