using Newtonsoft.Json.Linq;

namespace Tests.SDFFontTexture.Models;

internal sealed class Layout
{
    public Atlas? Atlas { get; set; }

    public Metrics? Metrics { get; set; }

    public Glyph[]? Glyphs { get; set; }

    public string? PngPath { get; set; }

    public bool IsValid => Atlas != null && Metrics != null && Glyphs != null && Glyphs.Length > 0;

    public static Layout Parse(string json, string pngPath)
    {
        JObject layoutObject = JObject.Parse(json);

        return new Layout
        {
            Atlas = layoutObject["atlas"]?.ToObject<Atlas>(),
            Metrics = layoutObject["metrics"]?.ToObject<Metrics>(),
            Glyphs = layoutObject["glyphs"]?.ToObject<Glyph[]>(),
            PngPath = pngPath
        };
    }
}
