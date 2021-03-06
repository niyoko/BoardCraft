﻿namespace BoardCraft.Input.Parsers.Schematic
{
    using Newtonsoft.Json.Linq;
    using NLog;
    using System;
    using Schematic = Models.Schematic;

    class SchematicParser
    {
        private readonly IComponentRepository _repository;
        public SchematicParser(IComponentRepository repository)
        {
            _repository = repository;
        }

        public Schematic Parse(JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var o = token as JObject;
            if (o == null)
            {
                throw new LibraryParseException("Token should have type object");
            }

            var jcomps = o.GetValue("components");
            var jcompsA = jcomps as JArray;
            if (jcompsA == null)
            {
                throw new LibraryParseException("components should be array");
            }

            var compParser = new ComponentParser(_repository);           
            var s = new Schematic();

            foreach (var c in jcompsA)
            {
                compParser.Parse(c, s);
            }

            var jconn = o.GetValue("connections");
            
            var jconnA = jconn as JArray;
            if (jconnA == null)
            {
                throw new Exception("Connections should be array");
            }

            var conParser = new ConnectionParser();
            foreach (var c in jconnA)
            {
                conParser.Parse(c, s);
            }

            return s;
        }
    }
}
