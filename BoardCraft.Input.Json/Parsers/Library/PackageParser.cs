namespace BoardCraft.Input.Parsers.Library
{
    using System;
    using System.Linq;
    using Models;
    using Newtonsoft.Json.Linq;

    class PackageParser
    {
        public Package Parse(JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var o = token as JObject;
            if (o == null)
            {
                throw new LibraryParseException("Parse error");
            }

            var jboundaries = o.GetValue("boundaries");
            var boundaries = new BoundariesParser().Parse(jboundaries);

            var name = o.GetValue("name").Value<string>();

            var d = o.GetValue("footprint");
            var da = d as JArray;
            if (da == null)
            {
                throw new LibraryParseException("");
            }
           
            var shapes = da.Select(dx => new ShapeParser().Parse(dx));

            var d2 = o.GetValue("pins");
            var da2 = d2 as JArray;
            if (da2 == null)
            {
               throw new LibraryParseException("");
            }

            var pins = da2.Select(x => new PinParser().Parse(x));

            return new Package(name, boundaries, pins, shapes);
        }
    }
}
