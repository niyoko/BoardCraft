namespace BoardCraft.Input
{
    using System.IO;
    using Models;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Parsers.Schematic;
    public class JsonInputParser : IInputParser
    {
        private readonly IComponentRepository _componentRepository;

        public JsonInputParser(IComponentRepository componentRepository)
        {
            _componentRepository = componentRepository;
        }

        public Schematic Parse(Stream inputStream)
        {
            using (var sr = new StreamReader(inputStream))
            {
                using (var jr = new JsonTextReader(sr))
                {
                    var sch = JToken.ReadFrom(jr);
                    return new SchematicParser(_componentRepository).Parse(sch);
                }
            }
        }
    }
}