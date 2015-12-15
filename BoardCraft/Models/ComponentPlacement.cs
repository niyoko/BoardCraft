﻿namespace BoardCraft.Models
{
    using System;
    using System.Collections.Generic;
    using Drawing;

    public sealed class ComponentPlacement
    {
        private readonly Dictionary<Component, PlacementInfo> _placement;
        private static readonly PlacementInfo DefaultPlacementInfo;

        static ComponentPlacement()
        {
            DefaultPlacementInfo = new PlacementInfo(Point.Origin, Orientation.Up);
        }

        public ComponentPlacement(Schematic schema)
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
        }

        public ComponentPlacement Clone()
        {
            var cloning = new ComponentPlacement(Schema);
            foreach (var placementInfo in _placement)
            {
                cloning._placement.Add(placementInfo.Key, placementInfo.Value);
            }

            return cloning;
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