using System;

namespace BoardCraft.Input
{
    public class LibraryNameDuplicateException : Exception
    {
        public LibraryNameDuplicateException(string message) : base(message) { }
    }
}
