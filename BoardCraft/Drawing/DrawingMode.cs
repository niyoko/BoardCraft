namespace BoardCraft.Drawing
{
    public enum DrawingMode
    {
        Component,
        BottomCopper,
        TopCopper,
        Pad,
        DrillHole,
        Via
#if DEBUG
        ,
        TopWave,
        BottomWave,
        BothWave
#endif
    }
}
