namespace BoardCraft.Input.Parsers.Schematic
{
    using System;
    using Models;
    using Newtonsoft.Json.Linq;

    class PinParser
    {
        public void Parse(JToken token, Schematic schematic, Connection connection)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (schematic == null)
            {
                throw new ArgumentNullException(nameof(schematic));
            }

            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            var t2 = token as JObject;

            if (t2 == null)
            {
                throw new Exception("Pin harus objek");
            }

            var c = t2.GetValue("component").Value<string>();
            var c1 = schematic.GetComponent(c);

            var pin = t2.GetValue("pin").Value<string>();
            connection.AddPin(c1, pin);
        }
    }
}
