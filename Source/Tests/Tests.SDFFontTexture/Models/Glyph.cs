namespace Tests.SDFFontTexture.Models;

internal sealed class Glyph
{
    public int UniCode { get; set; }

    public float Advance { get; set; }

    public Bounds PlaneBounds { get; set; }

    public Bounds AtlasBounds { get; set; }
}
