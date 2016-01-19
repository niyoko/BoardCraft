namespace BoardCraft.Input.Parsers.Library
{
    using System;
    using Models;
    using Newtonsoft.Json.Linq;
    using Drawing.PinStyles;
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

            var jstyle = o.GetValue("style");
            var st = (PinStyle)null;

            if (jstyle != null)
            {
                var style = jstyle.Value<string>();

                var sk = style.Split('-');
                
                if (sk[0] == "C")
                {
                    var padDiameter = int.Parse(sk[1]);
                    var drillDiameter = int.Parse(sk[2]);

                    st = new CirclePinStyle(padDiameter, drillDiameter);
                }

                if (sk[0] == "S")
                {
                    var sqSide = int.Parse(sk[1]);
                    var drillDiameter = int.Parse(sk[2]);

                    st = new SquarePinStyle(sqSide, drillDiameter);
                }

                /*
                if (sk[0] == "DIL")
                {
                    var w = int.Parse(sk[1]);
                    var d = int.Parse(sk[2]);

                    st = new DILPinStyle(w, d);
                }
                */
            }

            if (st == null)
            {
                st = new CirclePinStyle(70, 30);
            }

            return new Pin(name, pos, st);            
        }
    }
}
