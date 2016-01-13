﻿namespace BoardCraft.Models
{
    using System;
    using System.Collections.Generic;
    using Drawing;
    using Placement.GA;
    using System.Linq;

    public sealed class Board
    {
        private readonly Dictionary<Component, PlacementInfo> _placement;
        private static readonly PlacementInfo DefaultPlacementInfo;

        internal int[,] _wValues;
        internal double _cellSize;

        internal readonly ICollection<ICollection<IList<Point>>> _traces;

        static Board()
        {
            DefaultPlacementInfo = new PlacementInfo(Point.Origin, Orientation.Up);
        }

        public Board(Schematic schema)
        {
            if (schema == null)
            {
                throw new ArgumentNullException(nameof(schema));
            }

            Schema = schema;

            _placement = new Dictionary<Component, PlacementInfo>(schema.Components.Count);
            _bounds = new Dictionary<Component, Bounds>(schema.Components.Count);
            _pinLocations = new Dictionary<Component, Dictionary<string, Point>>(schema.Components.Count);
            _traces = new List<ICollection<IList<Point>>>();
        }

        public Schematic Schema { get; }

        public PlacementInfo GetComponentPlacement(Component component)
        {
            return _placement.ContainsKey(component) 
                ? _placement[component] 
                : DefaultPlacementInfo;
        }

        public void SetComponentPlacement(Component component, Point position, Orientation orientation)
        {
            var info = new PlacementInfo(position, orientation);
            SetComponentPlacement(component, info);
        }

        public void SetComponentPlacement(Component component, PlacementInfo info)
        {            
            _placement[component] = info;
            _bounds.Remove(component);
            _pinLocations.Remove(component);
        }

        public Board Clone()
        {
            var cloning = new Board(Schema);
            foreach (var placementInfo in _placement)
            {
                cloning._placement.Add(placementInfo.Key, placementInfo.Value);
            }           

            return cloning;
        }

        private readonly Dictionary<Component, Bounds> _bounds;
        private readonly Dictionary<Component, Dictionary<string, Point>> _pinLocations;

        public void CalculateBounds()
        {
            var cl = Schema.Components
                .Where(x => !_bounds.ContainsKey(x))
                .ToList();

            foreach (var c in cl)
            {
                var d = GetRealBound(c);
                _bounds.Add(c, d);
            }
        }

        public void CalculatePinLocations()
        {
            var cl = Schema.Components
                .Where(x => !_pinLocations.ContainsKey(x))
                .ToList();

            foreach (var c in cl)
            {
                var d = GetComponentPinLocation(c);
                _pinLocations.Add(c, d);
            }
        }

        public Bounds GetBounds(Component component)
        {
            Bounds b;
            if (_bounds.TryGetValue(component, out b))
            {
                return b;
            }

            var bx = GetRealBound(component);
            _bounds.Add(component, bx);

            return bx;
        }

        public Point GetPinLocation(Component component, string pin)
        {
            Dictionary<string, Point> p;
            if (_pinLocations.TryGetValue(component, out p))
            {
                return p[pin];
            }

            var ppx = GetComponentPinLocation(component);
            _pinLocations.Add(component, ppx);

            return ppx[pin];
        }

        public Size GetSize()
        {
            CalculateBounds();
            var cl = Schema.Components;
            var w = 0.0;
            var h = 0.0;

            foreach(var c in cl)
            {
                var b = GetBounds(c);
                if (b.Right > w)
                {
                    w = b.Right;
                }

                if (b.Top > h)
                {
                    h = b.Top;
                }
            }
            return new Size(w, h);            
        }

        private static readonly int[][] PointTransformer = new[]
        {
            new [] {1, 0, 0, 1},
            new [] {0, -1, 1, 0},
            new [] {-1, 0, 0, -1},
            new [] {0, 1, -1, 0}
        };

        private Bounds GetRealBound(Component component)
        {
            var metadata = GetComponentPlacement(component);
            var p = component.Package;

            double left = 0, top = 0, right = 0, bottom = 0;
            var package = p.Boundaries;
            switch (metadata.Orientation)
            {
                case Orientation.Up:
                    left = package.Left;
                    top = package.Top;
                    right = package.Right;
                    bottom = package.Bottom;
                    break;
                case Orientation.Left:
                    left = -package.Top;
                    top = package.Right;
                    right = -package.Bottom;
                    bottom = package.Left;
                    break;
                case Orientation.Down:
                    left = -package.Right;
                    top = -package.Bottom;
                    right = -package.Left;
                    bottom = -package.Top;
                    break;
                case Orientation.Right:
                    left = package.Bottom;
                    top = -package.Left;
                    right = package.Top;
                    bottom = -package.Right;
                    break;
            }

            left = metadata.Position.X + left;
            top = metadata.Position.Y + top;
            right = metadata.Position.X + right;
            bottom = metadata.Position.Y + bottom;

            return new Bounds(top, right, bottom, left);
        }

        private Dictionary<string, Point> GetComponentPinLocation(Component component)
        {
            var pack = component.Package;
            var plac = GetComponentPlacement(component);
            var ret = new Dictionary<string, Point>(pack.Pins.Count);

            var t = PointTransformer[(int)plac.Orientation];

            foreach (var pin in pack.Pins)
            {
                var pos = pin.Position;
                var x = t[0] * pos.X + t[1] * pos.Y;
                var y = t[2] * pos.X + t[3] * pos.Y;

                x += plac.Position.X;
                y += plac.Position.Y;

                ret.Add(pin.Name, new Point(x, y));
            }

            return ret;
        }

        public void Draw(ICanvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            canvas.Clear();
            foreach (var point in _placement)
            {
                var pos = point.Value.Position;
                var or = point.Value.Orientation;

                var ang = (Math.PI / 2.0) * ((int)or);
                canvas.Transform.PushMatrix();                
                canvas.Transform.Translate(pos.X, pos.Y);
                canvas.Transform.Rotate(ang);

                point.Key.Package.Draw(canvas);
                canvas.Transform.PopMatrix();
            }

            if (_traces.Any())
            {
                var tr = _traces.SelectMany(x => x);

                foreach (var t in tr)
                {
                    for (var i = 1; i < t.Count; i++)
                    {
                        canvas.DrawLine(t[i-1], t[i]);
                    }
                }
            }

            //_wValues = null;
            if (_wValues != null)
            {
                for (var i = 0; i < _wValues.GetLength(0); i++)
                {
                    for (var j = 0; j < _wValues.GetLength(1); j++)
                    {
                        if (_wValues[i, j] == -1)
                        {
                            canvas.DrawRectangle(new Point(i*_cellSize, j*_cellSize), _cellSize, _cellSize);
                        }
                    }
                }
            }
        }
    }
}