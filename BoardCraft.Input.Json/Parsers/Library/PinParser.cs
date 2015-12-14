namespace BoardCraft.Input.Parsers.Library
{
    using System;
    using Models;
    using Newtonsoft.Json.Linq;

    class PinParser
    {
        public Pin Parse(JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var o = token as JObject;
            if (o == null)
            {
                throw new LibraryParseException("");
            }

            var jname = o.GetValue("name");
            var name = jname.Value<string>();

            var jpos = o.GetValue("position");
            var pos = new PointParser().Parse(jpos);

            return new Pin(name, pos);            
        }
    }
}
