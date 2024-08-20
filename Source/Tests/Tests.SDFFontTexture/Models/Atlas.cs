namespace Tests.SDFFontTexture.Models;

internal sealed class Atlas
{
    public AtlasType Type { get; set; }

    public int DistanceRange { get; set; }

    public int DistanceRangeMiddle { get; set; }

    public float Size { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public string? YOrigin { get; set; }

    public Grid? Grid { get; set; }
}
