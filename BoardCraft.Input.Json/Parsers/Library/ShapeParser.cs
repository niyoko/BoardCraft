namespace BoardCraft.Input.Parsers.Library
{
    using System;
    using System.Collections.Generic;
    using Drawing.Shapes;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    class ShapeParser
    {
        private readonly IDictionary<string, Func<JObject, Shape>> _parsers;

        public ShapeParser()
        {
            _parsers = new Dictionary<string, Func<JObject, Shape>>
            {
                ["rectangle"] = ParseRectangle,
                ["circle"] = ParseCircle,
                ["ellipse"] = ParseEllipse,
                ["square"] = ParseSquare,
                ["line"] = ParseLine
            };
        }

        public Line ParseLine(JObject obj)
        {
            var jp1 = obj.GetValue("p1");
            var jp2 = obj.GetValue("p2");
            
            var p1 = new PointParser().Parse(jp1);
            var p2 = new PointParser().Parse(jp2);

            return new Line(p1, p2);
        }

        public Rectangle ParseRectangle(JObject obj)
        {
            var jbl = obj.GetValue("bottomLeft");
            var bl = new PointParser().Parse(jbl);
            var w = obj.GetValue("width").Value<double>();
            var h = obj.GetValue("height").Value<double>();

            return new Rectangle(bl, w, h);
        }

        public Circle ParseCircle(JObject obj)
        {
            var jcn = obj.GetValue("center");
            var cn = new PointParser().Parse(jcn);
            var radius = obj.GetValue("radius").Value<double>();

            return new Circle(cn, radius);
        }

        public Ellipse ParseEllipse(JObject obj)
        {
            var jcn = obj.GetValue("center");
            var cn = new PointParser().Parse(jcn);
            var xRadius = obj.GetValue("xRadius").Value<double>();
            var yRadius = obj.GetValue("yRadius").Value<double>();

            return new Ellipse(cn, xRadius, yRadius);
        }

        public Square ParseSquare(JObject obj)
        {
            var jbl = obj.GetValue("bottomLeft");
            var bl = new PointParser().Parse(jbl);

            var side = obj.GetValue("side").Value<double>();

            return new Square(bl, side);
        }

        public Shape Parse(JToken token)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            var o = token as JObject;

            if (o == null)
            {
                throw new LibraryParseException("Object expected");
            }

            var type = o.GetValue("type").Value<string>();
            if (_parsers.ContainsKey(type))
            {
                var parser = _parsers[type];
                var z = parser(o);
                return z;
            }

            throw new LibraryParseException("Invalid object");
        }
    }
}
