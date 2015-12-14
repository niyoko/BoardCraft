namespace BoardCraft.Input
{
    using System.Collections.Generic;
    using Models;

    public class Library
    {
        public Library(string name, IEnumerable<Package> packages)
        {
            Name = name;
            Packages = new List<Package>(packages);
        }

        public string Name { get; }
        public IList<Package> Packages { get; } 
    }
}
