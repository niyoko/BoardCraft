using BoardCraft.Models;
using Newtonsoft.Json.Linq;

namespace BoardCraft.Input.Parsers.Schematic
{
    using System.Collections.Generic;
    using Schematic = Models.Schematic;

    class ComponentParser
    {
        private readonly IComponentRepository _repository;
        public ComponentParser(IComponentRepository repository)
        {
            _repository = repository;
        }

        public void Parse(JToken token, Schematic schematic)
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

            var id = o.GetValue<string>("id");
            var pack = o.GetValue<string>("package");
            var jhp = o.GetValue("high-power");
            var hp = jhp?.Value<bool>() ?? false;

            var pack2 = _repository.GetPackage(pack);

            schematic.AddComponent(id, pack2, hp);            
        }
    }
}
