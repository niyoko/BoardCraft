﻿namespace BoardCraft.Input.Parsers.Library
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
                ["line"] = ParseLine,
                ["arcsegment"] = ParseArc
            };
        }

        public ArcSegment ParseArc(JObject obj)
        {
            var cp = obj.GetValue("center");
            var center = new PointParser().Parse(cp);

            var xrad = obj.GetValue("xRadius").Value<double>();
            var yrad = obj.GetValue("yRadius").Value<double>();

            var sAngle = obj.GetValue("startAngle").Value<double>();
            var eAngle = obj.GetValue("endAngle").Value<double>();

            const double convFactor = Math.PI / 180;

            return new ArcSegment(center, xrad, yrad, convFactor*sAngle, convFactor*eAngle);
        }

        public Line ParseLine(JObject obj)
        {
            var jP1 = obj.GetValue("p1");
            var jP2 = obj.GetValue("p2");
            
            var P1 = new PointParser().Parse(jP1);
            var P2 = new PointParser().Parse(jP2);

            return new Line(P1, P2);
        }

        public Rectangle ParseRectangle(JObject obj)
        {
            var jbl = obj.GetValue("position");
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
            var jbl = obj.GetValue("position");
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
