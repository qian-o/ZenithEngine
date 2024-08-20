namespace Tests.SDFFontTexture.Models;

internal struct CharacterRange(char begin, char end)
{
    public static readonly CharacterRange[] BaseRanges =
    [
        new((char)0x0020, (char)0x00FF), // Basic Latin + Latin-1 Supplement
        new((char)0x2000, (char)0x206F), // General Punctuation
        new((char)0x3000, (char)0x30FF), // CJK Symbols and Punctuation
        new((char)0x31F0, (char)0x31FF), // Katakana Phonetic Extensions
        new((char)0xFF00, (char)0xFFEF), // Halfwidth and Fullwidth Forms
        new((char)0xFFFD, (char)0xFFFD), // Invalid character
    ];

    public char Begin { get; set; } = begin;

    public char End { get; set; } = end;

    public override readonly string ToString()
    {
        return $"[0x{(int)Begin:X4}, 0x{(int)End:X4}]";
    }
}
