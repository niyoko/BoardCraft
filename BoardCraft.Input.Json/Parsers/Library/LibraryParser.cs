namespace BoardCraft.Input.Parsers.Library
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Models;
    using Newtonsoft.Json.Linq;
    using Library = Input.Library;

    class LibraryParser
    {
        public Library Parse(JToken token)
        {
            if (token == null)
            {
                throw new LibraryParseException(nameof(token));
            }

            var o = token as JObject;
            if (o == null)
            {
                throw new LibraryParseException("");
            }

            var jname = o.GetValue("name");
            var name = jname.Value<string>();

            var jpacks = o.GetValue("packages");
            var packs = new List<Package>();

            var packParser = new PackageParser();
            foreach (var jpack in jpacks)
            {                
                var pack = packParser.Parse(jpack);
                packs.Add(pack);
            }

            return new Library(name, packs);
        }
    }
}
