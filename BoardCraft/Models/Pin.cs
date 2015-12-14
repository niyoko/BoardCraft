namespace BoardCraft.Models
{
    using Drawing;

    public class Pin
    {
        public Pin(string name, Point position)
        {
            Name = name;
            Position = position;
        }

        public string Name { get; set; }

        public Point Position { get; set; }
    }
}