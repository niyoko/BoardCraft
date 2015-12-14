namespace BoardCraft.Input
{
    using System;

    internal class LibraryParseException : ParseException
    {
        public LibraryParseException(string message) : base(message)
        {
        }

        public LibraryParseException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}