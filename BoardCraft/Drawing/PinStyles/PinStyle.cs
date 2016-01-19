namespace BoardCraft.Drawing.PinStyles
{
    public abstract class PinStyle
    {
        public abstract void DrawTo(ICanvas canvas, Point position);
        public abstract void DrawDrillHole(ICanvas canvas, Point position);
    }
}
