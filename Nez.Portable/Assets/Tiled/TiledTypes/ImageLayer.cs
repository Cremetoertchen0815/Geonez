namespace Nez.Tiled;

public class TmxImageLayer : ITmxLayer
{
    //public int? Width;
    //public int? Height;

    public TmxImage Image;
    public TmxMap Map;
    public bool RepeatX { get; set; }
    public bool RepeatY { get; set; }
    public float Scale { get; set; }
    public string Name { get; set; }
    public bool Visible { get; set; }
    public float Opacity { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public float ParallaxFactorX { get; set; }
    public float ParallaxFactorY { get; set; }

    public PropertyDict Properties { get; set; }
}