namespace BoardCraft.Input.Parsers.Library
{
    using System;
    using System.Linq;
    using Models;
    using Newtonsoft.Json.Linq;

    class BoundariesParser
    {
        public Boundaries Parse(JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var arr = token as JArray;

            if (arr == null)
            {
                throw new LibraryParseException("Error parsing point");
            }

            if (arr.Count != 4)
            {
                throw new LibraryParseException("Point array expected two element");
            }

            var a = arr.Select(x => x.Value<double>()).ToArray();
            return new Boundaries(a[0], a[1], a[2], a[3]);
        }
    }
}
