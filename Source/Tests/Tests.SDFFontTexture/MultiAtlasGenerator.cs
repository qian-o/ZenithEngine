using System.Diagnostics;
using System.Globalization;
using System.Text;
using Tests.SDFFontTexture.Models;

namespace Tests.SDFFontTexture;

internal sealed class MultiAtlasGenerator
{
    public static readonly string ToolPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Tools", "msdf-atlas-gen.exe");
    public static readonly string OutputPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts");

    private readonly string _font;

    static MultiAtlasGenerator()
    {
        if (!File.Exists(ToolPath))
        {
            throw new FileNotFoundException(ToolPath);
        }

        if (!Directory.Exists(OutputPath))
        {
            Directory.CreateDirectory(OutputPath);
        }
    }

    public MultiAtlasGenerator(string font)
    {
        if (!File.Exists(font))
        {
            throw new FileNotFoundException(font);
        }

        _font = font;
    }

    public CharacterRange[] CharacterRanges { get; set; } = CharacterRange.BaseRanges;

    public bool IsAllGlyphs { get; set; }

    public AtlasType AtlasType { get; set; } = AtlasType.MSDF;

    public int EmSize { get; set; } = 64;

    public int Padding { get; set; } = 10;

    public Layout Generate()
    {
        string output = Path.Combine(OutputPath, Path.GetFileNameWithoutExtension(_font));

        if (!Directory.Exists(output))
        {
            Directory.CreateDirectory(output);
        }

        string outputJson = Path.Combine(output, $"{Path.GetFileNameWithoutExtension(_font)}.json");
        string outputPng = Path.Combine(output, $"{Path.GetFileNameWithoutExtension(_font)}.png");

        CultureInfo cultureInfo = CultureInfo.InvariantCulture;

        StringBuilder arguments = new();

        arguments.Append(cultureInfo, $" -font {_font}");

        if (!IsAllGlyphs)
        {
            string charset = string.Join(" ", CharacterRanges);

            string charsetFile = Path.Combine(output, $"{Path.GetFileNameWithoutExtension(_font)}.txt");

            File.Create(charsetFile).Dispose();

            File.WriteAllText(charsetFile, charset);

            arguments.Append(cultureInfo, $" -charset {charsetFile}");
        }

        arguments.Append(cultureInfo, $" -type {AtlasType.ToString().ToLower(cultureInfo)}");
        arguments.Append(cultureInfo, $" -size {EmSize}");
        arguments.Append(cultureInfo, $" -pxpadding {Padding}");
        arguments.Append(cultureInfo, $" -json {outputJson}");
        arguments.Append(cultureInfo, $" -imageout {outputPng}");

        ProcessStartInfo startInfo = new(ToolPath, arguments.ToString())
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using Process process = Process.Start(startInfo)!;

        process.WaitForExit();

        return Layout.Parse(File.ReadAllText(outputJson), outputPng);
    }
}
