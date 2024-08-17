using Graphics.Core;
using static StbTrueTypeSharp.StbTrueType;

namespace Tests.SDFFontTexture;

internal unsafe class FontController : DisposableObject
{
    private const int CacheSize = 256;

    private readonly stbtt_fontinfo _fontinfo;
    private readonly float _scale;
    private readonly int _padding;
    private readonly byte _onedgeValue;
    private readonly float _pixelDistScale;
    private readonly Dictionary<char, Character> _cache;

    public FontController(string fontPath, int fontIndex, int fontPixelHeight)
    {
        byte[] bytes = File.ReadAllBytes(fontPath);

        _fontinfo = CreateFont(bytes, stbtt_GetFontOffsetForIndex(bytes.AsPointer(), fontIndex));
        _scale = stbtt_ScaleForPixelHeight(_fontinfo, fontPixelHeight);
        _padding = 4;
        _onedgeValue = 128;
        _pixelDistScale = 64.0f;
        _cache = [];
    }

    public Character GetCharacter(char c)
    {
        if (_cache.TryGetValue(c, out Character character))
        {
            return character;
        }

        int width, height, xoff, yoff;
        byte* bitmap = stbtt_GetCodepointSDF(_fontinfo, _scale, c, _padding, _onedgeValue, _pixelDistScale, &width, &height, &xoff, &yoff);

        Character newCharacter = new(width, height, xoff, yoff);
        newCharacter.CopyPixels(bitmap);

        stbtt_FreeSDF(bitmap, null);

        if (_cache.Count >= CacheSize)
        {
            _cache.Clear();
        }

        _cache.Add(c, newCharacter);

        return newCharacter;
    }

    protected override void Destroy()
    {
        _cache.Clear();

        _fontinfo.Dispose();
    }
}
