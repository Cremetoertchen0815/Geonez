namespace Nez.Tiled;

public class TmxGroup : ITmxLayer
{
    public TmxList<TmxGroup> Groups;
    public TmxList<TmxImageLayer> ImageLayers;

    public TmxList<ITmxLayer> Layers;
    public TmxMap map;
    public TmxList<TmxObjectGroup> ObjectGroups;

    public TmxList<TmxLayer> TileLayers;
    public string Name { get; set; }
    public float Opacity { get; set; }
    public bool Visible { get; set; }
    public float OffsetX { get; set; }
    public float OffsetY { get; set; }
    public float ParallaxFactorX { get; set; }
    public float ParallaxFactorY { get; set; }

    public PropertyDict Properties { get; set; }
}