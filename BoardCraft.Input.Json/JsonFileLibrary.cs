namespace BoardCraft.Input
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Parsers.Library;

    public class JsonFileLibrary : ComponentRepositoryBase
    {
        private readonly string _path;
        private bool _initialized;

        public JsonFileLibrary(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            _path = path;
            _initialized = false;
            InternalLibraries = new Dictionary<string, Library>();
        }


        private IDictionary<string, Library> InternalLibraries { get; }
        public ICollection<Library> Libraries => InternalLibraries.Values;

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            Load();
        }

        public void Load()
        {
            InternalLibraries.Clear();

            var files = Directory.EnumerateFiles(_path);
            files = files.Where(f => f.EndsWith(".json") && !f.EndsWith(".schema.json"));

            try
            {
                foreach (var f in files)
                {
                    try
                    {
                        var result = ParseFile(f);
                        if (InternalLibraries.ContainsKey(result.Name))
                        {
                            throw new LibraryParseException($"Name {result.Name} already exist in library");
                        }
                        
                        InternalLibraries.Add(result.Name, result);                        
                    }
                    catch (Exception e)
                    {
                        if (e is JsonException)
                        {
                            var message = $"Json format invalid while parsing \"{f}\"";
                            throw new LibraryParseException(message, e);
                        }

                        throw;
                    }
                }

                var packs = Libraries.SelectMany(x => x.Packages);


                foreach (var pack in packs)
                {
                    try
                    {
                        RegisterPackage(pack);
                    }
                    catch (ArgumentException e)
                    {
                        throw new LibraryParseException($"Error while registering package {pack.Name}", e);
                    }
                }

                _initialized = true;
            }
            catch
            {
                ClearRegisteredPackage();
                InternalLibraries.Clear();

                throw;
            }
        }


        private static Library ParseFile(string filePath)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    using (var jr = new JsonTextReader(sr))
                    {
                        var lib = JToken.ReadFrom(jr);
                        var lib2 = new LibraryParser().Parse(lib);
                        return lib2;
                    }
                }
            }
        }
    }
}