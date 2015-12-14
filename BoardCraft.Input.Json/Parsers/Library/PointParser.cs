namespace BoardCraft.Input.Parsers.Library
{
    using System;
    using System.Linq;
    using Drawing;
    using Newtonsoft.Json.Linq;

    class PointParser
    {
        public Point Parse(JToken token)
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

            if (arr.Count != 2)
            {
                throw new LibraryParseException("Point array expected two element");
            }

            var a = arr.Select(x => x.Value<double>()).ToArray();
            return new Point(a[0], a[1]);
        }        
    }
}
