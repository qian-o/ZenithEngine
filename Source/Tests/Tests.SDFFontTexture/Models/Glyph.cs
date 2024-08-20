namespace Tests.SDFFontTexture.Models;

internal sealed class Glyph
{
    public int UniCode { get; set; }

    public int Advance { get; set; }

    public Bounds PlaneBounds { get; set; }

    public Bounds AtlasBounds { get; set; }
}
