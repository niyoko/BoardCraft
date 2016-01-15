namespace BoardCraft.Drawing.PinStyles
{
    public abstract class PinStyle
    {
        public abstract void DrawTo(ICanvas canvas, Point position);
    }
}
