﻿using ZenithEngine.Common;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.ImGuiWrapper;

internal static class Shaders
{
    public const string HLSL = @"
struct Constants
{
    float4x4 Projection;
};

struct VSInput
{
    float2 Position : POSITION0;
    
    float2 UV : TEXCOORD0;
    
    float4 Color : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    
    float2 UV : TEXCOORD0;
    
    float4 Color : COLOR0;
};

ConstantBuffer<Constants> constants : register(b0, space0);
SamplerState sampler0 : register(s0, space0);
Texture2D texture0 : register(t0, space1);

float3 SrgbToLinear(float3 srgb)
{
    return srgb * (srgb * (srgb * 0.305306011 + 0.682171111) + 0.012522878);
}

VSOutput VSMain(VSInput input)
{
    VSOutput output;
    
    output.Position = mul(float4(input.Position, 0.0, 1.0), constants.Projection);
    output.UV = input.UV;
    output.Color = input.Color;
    
#if 0
    output.Color.rgb = SrgbToLinear(output.Color.rgb);
#endif
    
    return output;
}

float4 PSMain(VSOutput input) : SV_TARGET
{
    return input.Color * texture0.Sample(sampler0, input.UV);
}
";

    public const string VSMain = "VSMain";

    public const string PSMain = "PSMain";

    public const string VSByLegacyHexString = "030223070006010000000E00240000000000000011000200010000000E00030000000000010000000F000C00000000000100000056534D61696E00000200000003000000040000000500000006000000070000000800000003000300050000005802000005000A0009000000747970652E436F6E7374616E744275666665722E436F6E7374616E747300000006000600090000000000000050726F6A656374696F6E00000500050008000000636F6E7374616E74730000000500070002000000696E2E7661722E504F534954494F4E30000000000500070003000000696E2E7661722E544558434F4F524430000000000500060004000000696E2E7661722E434F4C4F523000000005000700060000006F75742E7661722E544558434F4F52443000000005000600070000006F75742E7661722E434F4C4F52300000050004000100000056534D61696E000047000400050000000B0000000000000047000400020000001E0000000000000047000400030000001E0000000100000047000400040000001E0000000200000047000400060000001E0000000000000047000400070000001E0000000100000047000400080000002200000000000000470004000800000021000000000000004800050009000000000000002300000000000000480005000900000000000000070000001000000048000400090000000000000005000000470003000900000002000000150004000A00000020000000010000002B0004000A0000000B00000000000000160003000C000000200000002B0004000C0000000D000000000000002B0004000C0000000E0000000000803F170004000F0000000C0000000400000018000400100000000F000000040000001E00030009000000100000002000040011000000020000000900000017000400120000000C00000002000000200004001300000001000000120000002000040014000000010000000F0000002000040015000000030000000F000000200004001600000003000000120000001300020017000000210003001800000017000000200004001900000002000000100000003B0004001100000008000000020000003B0004001300000002000000010000003B0004001300000003000000010000003B0004001400000004000000010000003B0004001500000005000000030000003B0004001600000006000000030000003B0004001500000007000000030000003600050017000000010000000000000018000000F80002001A0000003D000400120000001B000000020000003D000400120000001C000000030000003D0004000F0000001D00000004000000510005000C0000001E0000001B00000000000000510005000C0000001F0000001B00000001000000500007000F000000200000001E0000001F0000000D0000000E000000410005001900000021000000080000000B0000003D000400100000002200000021000000910005000F0000002300000022000000200000003E00030005000000230000003E000300060000001C0000003E000300070000001D000000FD00010038000100";

    public const string VSByLinearHexString = "030223070006010000000E00310000000000000011000200010000000E00030000000000010000000F000C00000000000100000056534D61696E00000200000003000000040000000500000006000000070000000800000003000300050000005802000005000A0009000000747970652E436F6E7374616E744275666665722E436F6E7374616E747300000006000600090000000000000050726F6A656374696F6E00000500050008000000636F6E7374616E74730000000500070002000000696E2E7661722E504F534954494F4E30000000000500070003000000696E2E7661722E544558434F4F524430000000000500060004000000696E2E7661722E434F4C4F523000000005000700060000006F75742E7661722E544558434F4F52443000000005000600070000006F75742E7661722E434F4C4F52300000050004000100000056534D61696E000047000400050000000B0000000000000047000400020000001E0000000000000047000400030000001E0000000100000047000400040000001E0000000200000047000400060000001E0000000000000047000400070000001E0000000100000047000400080000002200000000000000470004000800000021000000000000004800050009000000000000002300000000000000480005000900000000000000070000001000000048000400090000000000000005000000470003000900000002000000150004000A00000020000000010000002B0004000A0000000B00000000000000160003000C000000200000002B0004000C0000000D000000000000002B0004000C0000000E0000000000803F2B0004000C0000000F00000012519C3E2B0004000C00000010000000C4A22E3F17000400110000000C000000030000002C00060011000000120000001000000010000000100000002B0004000C00000013000000C22C4D3C2C000600110000001400000013000000130000001300000017000400150000000C00000004000000180004001600000015000000040000001E00030009000000160000002000040017000000020000000900000017000400180000000C0000000200000020000400190000000100000018000000200004001A0000000100000015000000200004001B0000000300000015000000200004001C0000000300000018000000130002001D000000210003001E0000001D000000200004001F00000002000000160000003B0004001700000008000000020000003B0004001900000002000000010000003B0004001900000003000000010000003B0004001A00000004000000010000003B0004001B00000005000000030000003B0004001C00000006000000030000003B0004001B0000000700000003000000360005001D00000001000000000000001E000000F8000200200000003D0004001800000021000000020000003D0004001800000022000000030000003D000400150000002300000004000000510005000C000000240000002100000000000000510005000C00000025000000210000000100000050000700150000002600000024000000250000000D0000000E000000410005001F00000027000000080000000B0000003D00040016000000280000002700000091000500150000002900000028000000260000004F000800110000002A00000023000000230000000000000001000000020000008E000500110000002B0000002A0000000F00000081000500110000002C0000002B0000001200000085000500110000002D0000002A0000002C00000081000500110000002E0000002D0000001400000085000500110000002F0000002A0000002E0000004F0009001500000030000000230000002F000000040000000500000006000000030000003E00030005000000290000003E00030006000000220000003E0003000700000030000000FD00010038000100";

    public const string PSHexString = "030223070006010000000E001C0000000000000011000200010000000E00030000000000010000000F000A00040000000100000050534D61696E000002000000030000000400000005000000060000001000030001000000070000000300030005000000580200000500060007000000747970652E73616D706C657200000000050005000500000073616D706C657230000000000500060008000000747970652E32642E696D61676500000005000500060000007465787475726530000000000500070002000000696E2E7661722E544558434F4F524430000000000500060003000000696E2E7661722E434F4C4F523000000005000700040000006F75742E7661722E53565F544152474554000000050004000100000050534D61696E00000500070009000000747970652E73616D706C65642E696D616765000047000400020000001E0000000000000047000400030000001E0000000100000047000400040000001E00000000000000470004000500000022000000000000004700040005000000210000003C0000004700040006000000220000000100000047000400060000002100000014000000160003000A00000020000000170004000B0000000A000000040000001A00020007000000200004000C000000000000000700000019000900080000000A000000010000000200000000000000000000000100000000000000200004000D0000000000000008000000200004000E000000010000000B000000170004000F0000000A000000020000002000040010000000010000000F0000002000040011000000030000000B00000013000200120000002100030013000000120000001B00030009000000080000003B0004000C00000005000000000000003B0004000D00000006000000000000003B0004001000000002000000010000003B0004000E00000003000000010000003B0004001100000004000000030000003600050012000000010000000000000013000000F8000200140000003D0004000F00000015000000020000003D0004000B00000016000000030000003D0004000800000017000000060000003D0004000700000018000000050000005600050009000000190000001700000018000000570006000B0000001A000000190000001500000000000000850005000B0000001B000000160000001A0000003E000300040000001B000000FD00010038000100";

    public static void Get(ColorSpaceHandling colorSpaceHandling, out byte[] vs, out byte[] ps)
    {
        vs = colorSpaceHandling switch
        {
            ColorSpaceHandling.Legacy => Convert.FromHexString(VSByLegacyHexString),
            ColorSpaceHandling.Linear => Convert.FromHexString(VSByLinearHexString),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(colorSpaceHandling))
        };

        ps = Convert.FromHexString(PSHexString);
    }
}
