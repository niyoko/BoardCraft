using System;

namespace BoardCraft.Input
{
    public class PackageNameDuplicateException : Exception
    {
        public PackageNameDuplicateException(string message) : base(message) { }
    }
}
