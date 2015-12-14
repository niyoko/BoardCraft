namespace BoardCraft.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using Drawing;
    using Drawing.Shapes;

    public class Package
    {
        public Package(string name, Boundaries boundaries, IEnumerable<Pin> pins, IEnumerable<Shape> shapes)
        {
            if (pins == null)
            {
                throw new ArgumentNullException(nameof(pins));
            }

            if (shapes == null)
            {
                throw new ArgumentNullException(nameof(shapes));
            }

            Name = name;
            Boundaries = boundaries;

            var pinsInternal = new List<Pin>(pins);
            Pins = new ReadOnlyCollection<Pin>(pinsInternal);

            var shapesInternal = new List<Shape>(shapes);
            Shapes = new ReadOnlyCollection<Shape>(shapesInternal);
        }

        public string Name { get; }

        public IReadOnlyList<Pin> Pins { get; }

        public IReadOnlyList<Shape> Shapes { get; }

        public Boundaries Boundaries { get; }

        public void Draw(ICanvas canvas)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            foreach (var shape in Shapes)
            {
                shape.DrawTo(canvas);
            }
            

            foreach (var pin in Pins)
            {
                canvas.DrawEllipse(pin.Position, 0.5, 0.5);
            }
        }
    }
}