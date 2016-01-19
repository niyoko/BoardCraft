namespace BoardCraft.Drawing
{
    public enum DrawingMode
    {
        Component,
        BottomCopper,
        TopCopper,
        Pad,
        DrillHole,
        Via,
        BoardBoundary
#if DEBUG
        ,
        TopWave,
        BottomWave,
        BothWave
#endif
    }
}
