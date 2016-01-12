namespace BoardCraft.Models
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
            _boundsValid = false;
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

        private bool _boundsValid;
        private Size _size;
        private Bounds[] _bounds;

        public Size Size
        {
            get
            {
                if (!_boundsValid)
                {
                    CalculateBounds();
                }

                return _size;
            }
        }

        private static Bounds GetRealBound(PlacementInfo metadata, Package p)
        {
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

        private void CalculateBounds()
        {
            var componentList = Schema.Components.ToList();
            var count = componentList.Count;
            var d = new Bounds[count];

            for (var i = 0; i < count; i++)
            {
                var c = componentList[i];
                var p = GetComponentPlacement(c);
                d[i] = GetRealBound(p, c.Package);
            }

            _bounds = d;

            CalculateSize();

            _boundsValid = true;
        }

        private void CalculateSize()
        {
            double w = 0, h = 0;
            for (var i = 0; i < _bounds.Length; i++)
            {
                var b = _bounds[i];
                if (b.Right > w)
                {
                    w = b.Right;
                }

                if (b.Top > h)
                {
                    h = b.Top;
                }
            }

            _size = new Size(w, h);
        }

        public Bounds[] GetBounds()
        {
            if (!_boundsValid)
            {
                CalculateBounds();
            }

            return _bounds;
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
        }
    }
}