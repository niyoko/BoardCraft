using System;

namespace BoardCraft.Input.Parsers.Schematic
{
    using Newtonsoft.Json.Linq;
    using BoardCraft.Models;

    public class ConnectionParser
    {
        PinParser _pinParser = new PinParser();

        public void Parse(JToken token, Schematic schematic)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (schematic == null)
            {
                throw new ArgumentNullException(nameof(schematic));
            }

            var otoken = token as JObject;
            if (otoken == null)
            {
                throw new ArgumentException("connection must be an object");
            }

            var id = otoken.GetValue("id").Value<string>();

            var conn = schematic.AddConnection(id);
            var jpins = otoken.GetValue("connections");
            var pins = jpins as JArray;
            if (pins == null)
            {
                throw new Exception("connections must be array");
            }

            foreach (var pin in pins)
            {
                _pinParser.Parse(pin, schematic, conn);
            }            
        }
    }
}
