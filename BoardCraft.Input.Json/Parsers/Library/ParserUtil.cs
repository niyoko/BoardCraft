namespace BoardCraft.Input.Parsers.Library
{
    using System.Linq;

    internal static class ParserUtil
    {
        public static bool IsValidName(string name)
        {
            return name != null && 
                name.Select(c => char.IsLetterOrDigit(c) || c == '_')
                    .All(valid => valid);
        }
    }
}
