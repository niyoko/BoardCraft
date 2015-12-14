namespace BoardCraft.Input
{
    using System;
    using System.Collections.Generic;
    using Models;

    public abstract class ComponentRepositoryBase : IComponentRepository
    {
        private IDictionary<string, Package> PackageMetadata { get; }

        protected ComponentRepositoryBase()
        {
            PackageMetadata = new Dictionary<string, Package>();
        }

        public int PackageCount => PackageMetadata.Count;

        private static string NormalizeName(string name)
        {
            return name?.ToUpperInvariant();
        }

        protected void RegisterPackage(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException(nameof(package));
            }

            var name = package.Name;
            if (PackageMetadata.ContainsKey(name))
            {
                throw new ArgumentException($"Package with name {name} already exists", nameof(package));
            }

            PackageMetadata.Add(name, package);
        }

        protected void ClearRegisteredPackage()
        {
            PackageMetadata.Clear();
        }

        public virtual Package GetPackage(string name)
        {
            if (!PackageMetadata.ContainsKey(name))
            {
                throw new ArgumentException($"Package with name {name} does not exist", nameof(name));
            }

            return PackageMetadata[name];
        }
    }
}