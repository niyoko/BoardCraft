namespace BoardCraft.Input
{
    using System.IO;
    using Models;

    /// <summary>
    ///     Abstraction of reader and parser
    /// </summary>
    public interface IInputParser
    {
        Schematic Parse(Stream inputStream);
    }
}