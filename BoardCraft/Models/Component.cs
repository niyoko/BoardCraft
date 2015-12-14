namespace BoardCraft.Models
{
    using Drawing;

    /// <summary>
    ///     Represent component in the <see cref="Schematic" />, such as resistor, capacitor, IC, etc.
    /// </summary>
    public class Component
    {
        /// <summary>
        ///     Create instance of a <see cref="Component" />
        /// </summary>
        /// <param name="id"></param>
        /// <param name="package">Package type of this <see cref="Component" /></param>
        internal Component(string id, Package package)
        {
            Id = id;
            Package = package;
        }

        public string Id { get; }

        public Package Package { get; }

        public void Draw(ICanvas canvas)
        {
            canvas.DrawRectangle(new Point(-25, -50), 50, 100);
        }
    }
}