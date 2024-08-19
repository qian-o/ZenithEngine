using Graphics.Core;
using Graphics.Vulkan;
using static StbTrueTypeSharp.StbTrueType;

namespace Tests.SDFFontTexture;

internal sealed unsafe class FontController : DisposableObject
{
    private readonly GraphicsDevice _device;
    private readonly stbtt_fontinfo _fontinfo;
    private readonly float _scale;
    private readonly int _padding;
    private readonly byte _onedgeValue;
    private readonly float _pixelDistScale;
    private readonly Dictionary<char, Character> _characters;
    private readonly Dictionary<char, Texture> _textures;

    public FontController(GraphicsDevice device, string fontPath, int fontIndex, int fontPixelHeight)
    {
        byte[] bytes = File.ReadAllBytes(fontPath);

        _device = device;
        _fontinfo = CreateFont(bytes, stbtt_GetFontOffsetForIndex(bytes.AsPointer(), fontIndex));
        _scale = stbtt_ScaleForPixelHeight(_fontinfo, fontPixelHeight);
        _padding = 5;
        _onedgeValue = 180;
        _pixelDistScale = _onedgeValue / _padding;
        _characters = [];
        _textures = [];
    }

    public Character GetCharacter(char @char)
    {
        if (_characters.TryGetValue(@char, out Character character))
        {
            return character;
        }

        int width, height, xoff, yoff;
        byte* bitmap = stbtt_GetCodepointSDF(_fontinfo, _scale, @char, _padding, _onedgeValue, _pixelDistScale, &width, &height, &xoff, &yoff);

        Character newCharacter = new(@char, width, height, xoff, yoff);
        newCharacter.CopyPixels(bitmap);

        stbtt_FreeSDF(bitmap, null);

        _characters.Add(@char, newCharacter);

        return newCharacter;
    }

    public Texture GetTexture(char @char)
    {
        if (_textures.TryGetValue(@char, out Texture? texture))
        {
            return texture;
        }

        Character character = GetCharacter(@char);

        Texture newTexture = _device.ResourceFactory.CreateTexture(TextureDescription.Texture2D((uint)character.Width,
                                                                                                (uint)character.Height,
                                                                                                1,
                                                                                                PixelFormat.B8G8R8A8UNorm,
                                                                                                TextureUsage.Sampled | TextureUsage.GenerateMipmaps));
        _device.UpdateTexture(newTexture,
                              character.Pixels,
                              0,
                              0,
                              0,
                              (uint)character.Width,
                              (uint)character.Height,
                              1,
                              0,
                              0);

        _textures.Add(@char, newTexture);

        return newTexture;
    }

    protected override void Destroy()
    {
        foreach (Texture texture in _textures.Values)
        {
            texture.Dispose();
        }

        _textures.Clear();
        _characters.Clear();

        _fontinfo.Dispose();
    }
}
