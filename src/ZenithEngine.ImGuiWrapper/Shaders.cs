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

    public const string VSByLegacyHexDxil = "4458424392FC0583B00D2043ACC6D5DFEE1CC48101000000A0110000070000003C0000004C000000D400000060010000900200007C09000098090000534649300800000000000000000000004953473180000000030000000800000000000000680000000000000000000000030000000000000003030000000000000000000071000000000000000000000003000000010000000303000000000000000000007A000000000000000000000003000000020000000F0F000000000000504F534954494F4E00544558434F4F524400434F4C4F52004F5347318400000003000000080000000000000068000000000000000100000003000000000000000F00000000000000000000007400000000000000000000000300000001000000030C000000000000000000007D000000000000000000000003000000020000000F0000000000000053565F506F736974696F6E00544558434F4F524400434F4C4F5200005053563028010000340000000100000000000000000000000000000000000000FFFFFFFF010000000303000303000000000000000000000000000000280000000100000018000000020000000000000000000000000000000D000000000000003000000000504F534954494F4E00544558434F4F524400434F4C4F5200544558434F4F524400434F4C4F520056534D61696E0000010000000000000010000000010000000000000001004200030000000A000000000000000101420003000000130000000000000001024400030000000000000000000000010044030304000019000000000000000101420003020000220000000000000001024400030200000F0000000F0000000000000000000000100000002000000000000000000000000001000000020000000400000008000053544154E406000066000100B90100004458494C0601000010000000CC0600004243C0DE210C0000B00100000B82200002000000130000000781239141C80449061032399201840C250508191E048B628014450242920B42A41032143808184B0A32528848901420434688A500193242E4480E909122C4504151818CE183E58A0429460651180000080000001B8CE0FFFFFFFF074002A80D84F0FFFFFFFF03206D3086FFFFFFFF1F0009A800491800000300000013826042204C080600000000892000003500000032224809206485049322A484049322E384A19014124C8A8C0B84A44C107C23002500146600E608C0608E0029C620841442A61880105206A1A386CB9FB087907C6EA38A9598FCE2B61131C61854EE192E7FC21E42F243A019160205AB108A3042AD14838C31E8DD365CFE843D84E4AF84E4509140A491F310D184101212082904232C923C68B8FC097B08C95F0969439A011184903247109482114A2AD9818061046248827CDBE148D302600E35F9D21451C2E4734E234D40334928D0471D8E342D00E650932F388D3401CD24A160134F07020000131472C08774608736688779680372C0870DAF500E6DD00E7A500E6D000F7A300772A0077320076D900E71A0077320076D900E78A0077320076D900E7160077A300772D006E9300772A0077320076D900E7640077A600774D006E6100776A0077320076D600E7320077A300772D006E6600774A0077640076DE00E78A0077160077A300772A007764007439E0008000000000000000000863C06100001000000000000000C79102000040000000000000018F23440000C0000000000000030E4798000080000000000000060C82301011000000000000000C090C702022000000000000000802C1013000000321E981419114C908C092647C604432225500C230005185004855006E55012E55110055614544A6204A0080AA10C68CF00109F01A03E03407E2C87619E070020300000100121100C4050000079180000940000001A034C90460213C43120C31B4381934BB30BA32B4B018971C1718171A1B999C9014121CB09AB29CBB9218349D910041304E2982010C8066120260844B241300C0A76731B0684202608DDC664ECCD6D8E2ECC8D6E6E8240281B1043590C6360800D41B38100000798206C191528B937B5B231BAB437B70902B14C100866C3304DC30481682608843341209E0D081149546558D70681C126089CB6812096CAD82030DB86C280328D9B2008C00660C3607CDF86000C360C83170613048FDB108C010DABA9A6B0343722504F534954494F4E1384829A2014D586C0982014D606A1AA362C86199C011AA4011A0C6A60A0C11A10A12AC21A7A7A9222DAB00C6D70066890066830A8C180066BC062E889E9496A82505C130402DA205471B06191DEE00CD0200DD0608003090DE460C3C0066E30075CA6ACBEA0DEE6D2E8D2DEDC260805B66131EAE00CEC200DE0608003030DE460C332B4C119A0411AA8C1A006031AACC186457A83334083345083010E243490830DC31DE0411E6C18E8400F800D8557067BF00034CCD8DEC2E8E6260844C422CD6D8E6E6E82404834E6D2CEBED8C868CCA59D7DCDD14D10886903D2077EF007A0100A7E200AA350858DCDAECD258DACCC8D6E4A105421C373B12B939B4B7B739B12104DC8F05CECC2D8ECCAE4A604461D323C9739B430B232B9A637B232B6290152860CCF45AE6CEEAD4E6EAC6C6E4AE05422C373A1CB832B0B72737BA30BA34B7B739B9B22706150870CCFC52EADEC2E896C8A2E8CAE6C4A300675C8F05CCADCE8E4F2A0DED2DCE8E6A6047BD0850CCF65ECADCE8DAE4C6E6E4A300A000000791800004C0000003308801CC4E11C6614013D88433884C38C4280077978077398710CE6000FED100EF4800E330C421EC2C11DCEA11C6630053D88433884831BCC033DC8433D8C033DCC788C7470077B08077948877070077A700376788770208719CC110EEC900EE1300F6E300FE3F00EF0500E3310C41DDE211CD8211DC2611E6630893BBC833BD04339B4033CBC833C84033BCCF0147660077B680737688772680737808770908770600776280776F8057678877780875F08877118877298877998812CEEF00EEEE00EF5C00EEC300362C8A11CE4A11CCCA11CE4A11CDC611CCA211CC4811DCA6106D6904339C84339984339C84339B8C33894433888033B94C32FBC833CFC823BD4033BB0C38CC821077C70037210877370037B080779608770C88777A8077A98813CE4800F6E400FE5D00EF0000000712000001F0000000660BCAC09208D1550C3E53B8F0F348D3301131102CDB01036B00D97EF3CBE1050454144A5030C25610002E617B76D06DD70F9CEE30B11014C440834C3427C91C36C4833208D6101D370F9CEE32F0E3088CD434D7E71DB26500D97EF3CBE3439118152D3434D7E71DB46200D97EF3CFE4444130244985FDC3600000000000000484153481400000000000000170E6BF891AF601987A156CBBFAD80BC4458494C0008000066000100000200004458494C0601000010000000E80700004243C0DE210C0000F70100000B82200002000000130000000781239141C80449061032399201840C250508191E048B628014450242920B42A41032143808184B0A32528848901420434688A500193242E4480E909122C4504151818CE183E58A0429460651180000080000001B8CE0FFFFFFFF074002A80D84F0FFFFFFFF03206D3086FFFFFFFF1F0009A800491800000300000013826042204C080600000000892000003500000032224809206485049322A484049322E384A19014124C8A8C0B84A44C107C23002500146600E608C0608E0029C620841442A61880105206A1A386CB9FB087907C6EA38A9598FCE2B61131C61854EE192E7FC21E42F243A019160205AB108A3042AD14838C31E8DD365CFE843D84E4AF84E4509140A491F310D184101212082904232C923C68B8FC097B08C95F0969439A011184903247109482114A2AD9818061046248827CDBE148D302600E35F9D21451C2E4734E234D40334928D0471D8E342D00E650932F388D3401CD24A160134F07020000131472C08774608736688779680372C0870DAF500E6DD00E7A500E6D000F7A300772A0077320076D900E71A0077320076D900E78A0077320076D900E7160077A300772D006E9300772A0077320076D900E7640077A600774D006E6100776A0077320076D600E7320077A300772D006E6600774A0077640076DE00E78A0077160077A300772A007764007439E0000000000000000000000863C06100001000000000000000C79102000040000000000000018F23440000C0000000000000030E4798000080000000000000060C82301011000000000000000C090C702022000000000000000802C1010000000321E981419114C908C092647C604432225500C053102508001655004E541A52446008AA010CA80FA0C00F9B11C86791E0080C00000400484403000410100000079180000620000001A034C90460213C43120C31B4381934BB30BA32B4B018971C1718171A1B999C9014121CB09AB29CBB9218349D910041304E2982010C8066120260844B241180C0A76731B0684202608843241E82802130462D980280BA32843036C089C0D04003CC004C1AB3604D104410068584D3585A5B911817A9A4AA24A7A729A2014CF04A1803604CA04A188260804B341D0B40D8B525917760D99726D44A88AB0869E9EA488362C43675DD83564C3B54D108886C5D013D393D404A190260884B341D0C460C3F281817561D71006DF35061B06CE23032E53565F506F736974696F6E138462DAB02866609D011606431828D7186C5886CEBAB06CC8866BDBB07C60605D583684C1778DC186010DD2400D360C65B006C08662A2D80002AAB0B1D9B5B9A49195B9D14D09822A64782E76657273696F6E5302A209199E8B5D189B5D99DC94C0A84386E7328716465626D7F44656C6362540CA90E1B9C895CDBDD5C98D95CD4D099E3A64782E76696577496453746174655382A80E199E4B991B9D5C1ED45B9A1BDDDC94800D0000791800004C0000003308801CC4E11C6614013D88433884C38C4280077978077398710CE6000FED100EF4800E330C421EC2C11DCEA11C6630053D88433884831BCC033DC8433D8C033DCC788C7470077B08077948877070077A700376788770208719CC110EEC900EE1300F6E300FE3F00EF0500E3310C41DDE211CD8211DC2611E6630893BBC833BD04339B4033CBC833C84033BCCF0147660077B680737688772680737808770908770600776280776F8057678877780875F08877118877298877998812CEEF00EEEE00EF5C00EEC300362C8A11CE4A11CCCA11CE4A11CDC611CCA211CC4811DCA6106D6904339C84339984339C84339B8C33894433888033B94C32FBC833CFC823BD4033BB0C38CC821077C70037210877370037B080779608770C88777A8077A98813CE4800F6E400FE5D00EF0000000712000001F0000000660BCAC09208D1550C3E53B8F0F348D3301131102CDB01036B00D97EF3CBE1050454144A5030C25610002E617B76D06DD70F9CEE30B11014C440834C3427C91C36C4833208D6101D370F9CEE32F0E3088CD434D7E71DB26500D97EF3CBE3439118152D3434D7E71DB46200D97EF3CFE4444130244985FDC36000000612000007A0000001304412C1000000009000000444AA11066008AABEC4A76A060074A834A09501D01203A07A1284A44710EA15B00000000230608008260607187800D230607008260207947208C1824000882813106CDA551CC8841028020181864E060DBD18C18240008828151064FC655CE884102802018186600691DF58C18240008828171065118781834629000200806061A4862F02DD188410280201818693081011868D2884102802018186A4085411834D388C1018020183469202562309A1000A30942309A3008A309C4306270002008068D1B5C8E1A8C2604C06882108C260CC26802318C181C000882413307DCA406A30901309A2004A30983309A400CE644F219314000100483070FC62052023302E81844C967C4000140100C9E3D30038A092C40A063D2259F11030400413078FC200D2E27B040818E519A7C460C100004C1E0090536D0A0C002063A2306090082608094421CF4411FDC81326290002008064829C4411FF4411A1C2306090082608094421CF4411FD801316290002008064829C4411FF4011D042306090082608094421C80421FDCC1376290002008064829C40128F4411A782306090082608094421CE8411FDCC1188C182400088201520A71A0077D900662306290002008064829C4811EF4811D84C188410280201820A510077AD007740006080000000000";

    public const string VSByLinearHexDxil = "44584243B6AF079C7A452348188C50D69E4203730100000000120000070000003C0000004C000000D4000000600100009002000084090000A0090000534649300800000000000000000000004953473180000000030000000800000000000000680000000000000000000000030000000000000003030000000000000000000071000000000000000000000003000000010000000303000000000000000000007A000000000000000000000003000000020000000F0F000000000000504F534954494F4E00544558434F4F524400434F4C4F52004F5347318400000003000000080000000000000068000000000000000100000003000000000000000F00000000000000000000007400000000000000000000000300000001000000030C000000000000000000007D000000000000000000000003000000020000000F0000000000000053565F506F736974696F6E00544558434F4F524400434F4C4F5200005053563028010000340000000100000000000000000000000000000000000000FFFFFFFF010000000303000303000000000000000000000000000000280000000100000018000000020000000000000000000000000000000D000000000000003000000000504F534954494F4E00544558434F4F524400434F4C4F5200544558434F4F524400434F4C4F520056534D61696E0000010000000000000010000000010000000000000001004200030000000A000000000000000101420003000000130000000000000001024400030000000000000000000000010044030304000019000000000000000101420003020000220000000000000001024400030200000F0000000F0000000000000000000000100000002000000000000000000000000001000000020000000400000008000053544154EC06000066000100BB0100004458494C0601000010000000D40600004243C0DE210C0000B20100000B82200002000000130000000781239141C80449061032399201840C250508191E048B628014450242920B42A41032143808184B0A32528848901420434688A500193242E4480E909122C4504151818CE183E58A0429460651180000080000001B8CE0FFFFFFFF074002A80D84F0FFFFFFFF03206D3086FFFFFFFF1F0009A800491800000300000013826042204C080600000000892000003500000032224809206485049322A484049322E384A19014124C8A8C0B84A44C107C23002500146600E608C0608E0029C620841442A61880105206A1A386CB9FB087907C6EA38A9598FCE2B61131C61854EE192E7FC21E42F243A019160205AB108A3042AD14838C31E8DD365CFE843D84E4AF84E4509140A491F310D184101212082904232C923C68B8FC097B08C95F0969439A011184903247109482114A2AD9818061046248827CDBE148D302600E35F9D21451C2E4734E234D40334928D0471D8E342D00E650932F388D3401CD24A160134F07020000131472C08774608736688779680372C0870DAF500E6DD00E7A500E6D000F7A300772A0077320076D900E71A0077320076D900E78A0077320076D900E7160077A300772D006E9300772A0077320076D900E7640077A600774D006E6100776A0077320076D600E7320077A300772D006E6600774A0077640076DE00E78A0077160077A300772A007764007439E0008000000000000000000863C06100001000000000000000C79102000040000000000000018F23440000C0000000000000030E4798000080000000000000060C82301011000000000000000C090C702022000000000000000802C1014000000321E981419114C908C092647C604432225500C230005185004855006E55012E5512EE557104541A52446008AA010CA80F60C00F11900EA3300E4C77218E679000002030000111002C10004050000000079180000950000001A034C90460213C43120C31B4381934BB30BA32B4B018971C1718171A1B999C9014121CB09AB29CBB9218349D910041304E2982010C8066120260844B241300C0A76731B06842026081DC764ECCD6D8E2ECC8D6E6E8240281B1043590C6360800D41B38100000798206C1A1528B937B5B231BAB437B70902B14C100866C3304DC30481682608843341209E0D081149546558D70681C12608DCB6812096CAD82030DB86C280328D9B2008C00660C3607CDF86000C360C8317061304AFDB108C010DABA9A6B0343722504F534954494F4E1384A29A2014D686C0982014D706A1AA362C86199C011AA4011A0C6A60A0C11A10A12AC21A7A7A9222DAB00C6D70066890066830A8C180066BC062E889E9496A825060130402DA205471B06191DEE00CD0200DD0608003090DE460C3C0066E30075CA6ACBEA0DEE6D2E8D2DEDC260845B66131EAE00CEC200DE0608003030DE460C332B4C119A0411AA8C1A006031AACC186457A83334083345083010E243490830DC31DE0411E6C18E8400F800D8557067BF00034CCD8DEC2E8E6260844C422CD6D8E6E6E82404834E6D2CEBED8C82608C444632EEDEC6B8E6E8240501B903EF0833F00855010855120852A6C6C766D2E6964656E745382A00A199E8B5D99DC5CDA9BDB9480684286E76217C6665726372530EA90E1B9CCA1859195C935BD9195B14D09903264782E7265736F75726365735302A712199E0B5D1E5C59909BDB1B5D185DDA9BDBDC14810B833A64782E7669657749645374617465538231A84386E752E646279707F596E646373725D8832E64782E636F756E7465727353025200000000791800004C0000003308801CC4E11C6614013D88433884C38C4280077978077398710CE6000FED100EF4800E330C421EC2C11DCEA11C6630053D88433884831BCC033DC8433D8C033DCC788C7470077B08077948877070077A700376788770208719CC110EEC900EE1300F6E300FE3F00EF0500E3310C41DDE211CD8211DC2611E6630893BBC833BD04339B4033CBC833C84033BCCF0147660077B680737688772680737808770908770600776280776F8057678877780875F08877118877298877998812CEEF00EEEE00EF5C00EEC300362C8A11CE4A11CCCA11CE4A11CDC611CCA211CC4811DCA6106D6904339C84339984339C84339B8C33894433888033B94C32FBC833CFC823BD4033BB0C30CC421077C70037A288776808719D1430EF8E006E4200EE7E006F6100EF2C00EE1900FEF500FF4000000712000001F0000000660BCAC09208D1550C3E53B8F0F348D3301131102CDB01036B00D97EF3CBE1050454144A5030C25610002E617B76D06DD70F9CEE30B11014C440834C3427C91C36C4833208D6101D370F9CEE32F0E3088CD434D7E71DB26500D97EF3CBE3439118152D3434D7E71DB46200D97EF3CFE4444130244985FDC36000000000000004841534814000000000000008D850B1CDF7885B84A25C0F98D50C0554458494C5808000066000100160200004458494C0601000010000000400800004243C0DE210C00000D0200000B82200002000000130000000781239141C80449061032399201840C250508191E048B628014450242920B42A41032143808184B0A32528848901420434688A500193242E4480E909122C4504151818CE183E58A0429460651180000080000001B8CE0FFFFFFFF074002A80D84F0FFFFFFFF03206D3086FFFFFFFF1F0009A800491800000300000013826042204C080600000000892000003500000032224809206485049322A484049322E384A19014124C8A8C0B84A44C107C23002500146600E608C0608E0029C620841442A61880105206A1A386CB9FB087907C6EA38A9598FCE2B61131C61854EE192E7FC21E42F243A019160205AB108A3042AD14838C31E8DD365CFE843D84E4AF84E4509140A491F310D184101212082904232C923C68B8FC097B08C95F0969439A011184903247109482114A2AD9818061046248827CDBE148D302600E35F9D21451C2E4734E234D40334928D0471D8E342D00E650932F388D3401CD24A160134F07020000131472C08774608736688779680372C0870DAF500E6DD00E7A500E6D000F7A300772A0077320076D900E71A0077320076D900E78A0077320076D900E7160077A300772D006E9300772A0077320076D900E7640077A600774D006E6100776A0077320076D600E7320077A300772D006E6600774A0077640076DE00E78A0077160077A300772A007764007439E0000000000000000000000863C06100001000000000000000C79102000040000000000000018F23440000C0000000000000030E4798000080000000000000060C82301011000000000000000C090C702022000000000000000802C1010000000321E981419114C908C092647C604432225500C053102508001655004E541A52446008AA010CA80FA0C00F9B11C86791E0080C00000400484403000410100000079180000620000001A034C90460213C43120C31B4381934BB30BA32B4B018971C1718171A1B999C9014121CB09AB29CBB9218349D910041304E2982010C8066120260844B241180C0A76731B0684202608843241E82802130462D980280BA32843036C089C0D04003CC004C1AB3604D104410068584D3585A5B911817A9A4AA24A7A729A2014CF04A1803604CA04A188260804B341D0B40D8B525917760D99726D44A88AB0869E9EA488362C43675DD83564C3B54D108886C5D013D393D404A190260884B341D0C460C3F281817561D71006DF35061B06CE23032E53565F506F736974696F6E138462DAB02866609D011606431828D7186C5886CEBAB06CC8866BDBB07C60605D583684C1778DC186010DD2400D360C65B006C08662A2D80002AAB0B1D9B5B9A49195B9D14D09822A64782E76657273696F6E5302A209199E8B5D189B5D99DC94C0A84386E7328716465626D7F44656C6362540CA90E1B9C895CDBDD5C98D95CD4D099E3A64782E76696577496453746174655382A80E199E4B991B9D5C1ED45B9A1BDDDC94800D0000791800004C0000003308801CC4E11C6614013D88433884C38C4280077978077398710CE6000FED100EF4800E330C421EC2C11DCEA11C6630053D88433884831BCC033DC8433D8C033DCC788C7470077B08077948877070077A700376788770208719CC110EEC900EE1300F6E300FE3F00EF0500E3310C41DDE211CD8211DC2611E6630893BBC833BD04339B4033CBC833C84033BCCF0147660077B680737688772680737808770908770600776280776F8057678877780875F08877118877298877998812CEEF00EEEE00EF5C00EEC300362C8A11CE4A11CCCA11CE4A11CDC611CCA211CC4811DCA6106D6904339C84339984339C84339B8C33894433888033B94C32FBC833CFC823BD4033BB0C30CC421077C70037A288776808719D1430EF8E006E4200EE7E006F6100EF2C00EE1900FEF500FF4000000712000001F0000000660BCAC09208D1550C3E53B8F0F348D3301131102CDB01036B00D97EF3CBE1050454144A5030C25610002E617B76D06DD70F9CEE30B11014C440834C3427C91C36C4833208D6101D370F9CEE32F0E3088CD434D7E71DB26500D97EF3CBE3439118152D3434D7E71DB46200D97EF3CFE4444130244985FDC3600000061200000900000001304412C100000000E000000544700A8940091D2288542980128AEB22BD981821DA03146209AAB4E7A6304A48DF6F247710E015B88CE41288A12D1182390511A4F3F0000230608008260607D87B03D2306070082602085C11110230609008260609841A3751733629000200806C619389B173523060900826060A0C1C37D9833629000200806461A401D185CCF884102802018186A109141186CD0884102802018186B2095811854D1884102802018186C308DC11874D2884102802018186D409101195CD388C101802018346C202565309A1000A30942309A3008A309C4306270002008064D1C5C4E1B8C2604C06882108C260CC26802318C181C000882416307DCD406A30901309A2004A30983309A400CE644F219314000100483670FC62052023302E81844C967C4000140100C1E3F30038A092C40A063D2259F1103040041307842210D2E27B040818E519A7C460C100004C1E0210536D0A0C002063A360666201F1B8333908F8D011AC8C70636808F0D6D001F1BDC003E36A4817C6C4803F9D89006F2B1610EE063031DC0C7863A808F0D6F201F1BDE403E36BC817C460C120004C100B9055078855748856CC4200140100C905B008557780551B0460C120004C100B90550788557408569C4200140100C905B00855778055380460C120004C100B9055090855748053718314800100403E4164041165E4114DA60C4200140100C905B00055678855418460C120004C100B90550608557100561C4200140100C905B00055678055408460C120004C100B905506085573085374000000000000000";

    public const string PSHexDxil = "445842431A36000936AE5DB5C5740EE749AF235B01000000B0100000070000003C0000004C000000D80000001401000024020000080900002409000053464930080000000000000000000000495347318400000003000000080000000000000068000000000000000100000003000000000000000F000000000000000000000074000000000000000000000003000000010000000303000000000000000000007D000000000000000000000003000000020000000F0F00000000000053565F506F736974696F6E00544558434F4F524400434F4C4F5200004F5347313400000001000000080000000000000028000000000000004000000003000000000000000F0000000000000053565F5461726765740000005053563008010000340000000000000000000000000000000000000000000000FFFFFFFF000000000301000301000000000000000000000000000000100000000200000018000000010000000000000000000000000000000E000000000000000300000001000000000000000000000002000000000000001800000000544558434F4F524400434F4C4F520050534D61696E000001000000000000001000000000000000000000000100440303040000010000000000000001014200030200000A00000000000000010244000302000000000000000000000100441003000000000000000000000000000000000000000F0000000F00000000000000000000000100000002000000040000000800000053544154DC06000066000000B70100004458494C0601000010000000C40600004243C0DE210C0000AE0100000B82200002000000130000000781239141C80449061032399201840C250508191E048B628014450242920B42A41032143808184B0A32528848901420434688A500193242E4480E909122C4504151818CE183E58A0429460651180000080000001B8CE0FFFFFFFF074002A80D84F0FFFFFFFF03206D3086FFFFFFFF1F0009A800491800000300000013826042204C080600000000892000004F00000032224809206485049322A484049322E384A19014124C8A8C0B84A44C107823002500146600E608C0608E0029C620841442A61880105206A19B86CB9FB08790FC95905662F28BDB46C51863102AF70C973F610F21F921D00C0B8182551845181B630C42C8A076DB70F913F61092BF12924345029146CE43441342484820A4108CB047F0A0E1F227EC21247F25A40D6906441042CA1C41500A46249944070286118861A636180776088779980737A08572C0077AA8077928073920053EB0877218077A780779E0037360877708077A600330A0033F00033FD0033D688774808779F8057AC807782807149099C4601CD8211CE6611EDC8016CA011FE8A11EE4A11CE48014F8C01ECA611CE8E11DE4810FCC811DDE211CE8810DC0800EFC000CFC00091753BE499A224A987C16609E8588D80998081410DAE94000000000131472C08774608736688779680372C0870DAF500E6DD00E7A500E6D000F7A300772A0077320076D900E71A0077320076D900E78A0077320076D900E7160077A300772D006E9300772A0077320076D900E7640077A600774D006E6100776A0077320076D600E7320077A300772D006E6600774A0077640076DE00E78A0077160077A300772A007764007439E0008000000000000000000863C06100001000000000000000C79102000040000000000000018F23440000C0000000000000030E4818000080000000000000060C843010110000000000000004016080010000000321E981419114C908C092647C604432225500C230045501265501E855030544AA20C0A6104A0080A84F40C00F11900EA63390C010000F03C001008040200000079180000840000001A034C90460213C43120C31B4381934BB30BA32B4B018971C1718171A1B999C9014121CB09AB29CBB9218349D910041304C2982010C7066120260804B241300C0A70731B06842026085A4584AE0C8FAE4EAE0C66824024130442D92018CD86C45096C11818C3D9103C1304CE223217D606C7562607B3013122C9300603D8104C1B080800A80982006C00360CC6756D08B00DC360651384EEDA106C34A0A69AC2D2DCB84C597D41BDCDA5D1A5BDB94D108A678250401B0263825044138442DAB018DE0706612006831818630010A12AC21A7A7A92229A2014D304815836086770061B96A10CBE3108033318CC601803346031F4C4F424354120980DC219ACC1868551836F0CC2C00C063160C6800D360C6490066DC064CAEA8B2A4CEEAC8C6E8250501B16E30D3E3808833118C4C0180336D810C4C186C10DE400D85058DD1C54000D33B6B730BAB90902D1B048739BA39B9B20100E8DB9B4B32F36321A7369675F737444E8CAF0BEDCDEE4DA36287560077780077940E8811DECC150858DCDAECD258DACCC8D6E4A105421C373B12B939B4B7B739B12104DC8F05CECC2D8ECCAE4A604461D323C9739B430B232B9A637B232B6290152860CCF45AE6CEEAD4E6EAC6C6E4A405522C373A1CB832B0B72737BA30BA34B7B739B9B126475C8F05CECD2CAEE92C8A6E8C2E8CAA6045B1D323C9732373AB93CA8B73437BAB929C11C7421C373197BAB73A32B939B9B12EC0100791800004C0000003308801CC4E11C6614013D88433884C38C4280077978077398710CE6000FED100EF4800E330C421EC2C11DCEA11C6630053D88433884831BCC033DC8433D8C033DCC788C7470077B08077948877070077A700376788770208719CC110EEC900EE1300F6E300FE3F00EF0500E3310C41DDE211CD8211DC2611E6630893BBC833BD04339B4033CBC833C84033BCCF0147660077B680737688772680737808770908770600776280776F8057678877780875F08877118877298877998812CEEF00EEEE00EF5C00EEC300362C8A11CE4A11CCCA11CE4A11CDC611CCA211CC4811DCA6106D6904339C84339984339C84339B8C33894433888033B94C32FBC833CFC823BD4033BB0C30CC421077C70037A288776808719D1430EF8E006E4200EE7E006F6100EF2C00EE1900FEF500FF400000071200000190000000660A4AC09208D1150C3E53B8F0F348D3301131102CDB01056D00D97EF3CBE1011C0448440332CC41739CC863403D21816300D97EF3CFEE20083D83CD4E417B76D03D070F9CEE34B00F32C845FDCB60954C3E53B8F2F4D4E44A0D4F450935FDC3600000000000000484153481400000000000000377E26AE82711120DE9908E2263E7C784458494C8407000066000000E10100004458494C06010000100000006C0700004243C0DE210C0000D80100000B82200002000000130000000781239141C80449061032399201840C250508191E048B628014450242920B42A41032143808184B0A32528848901420434688A500193242E4480E909122C4504151818CE183E58A0429460651180000080000001B8CE0FFFFFFFF074002A80D84F0FFFFFFFF03206D3086FFFFFFFF1F0009A800491800000300000013826042204C080600000000892000004F00000032224809206485049322A484049322E384A19014124C8A8C0B84A44C107823002500146600E608C0608E0029C620841442A61880105206A19B86CB9FB08790FC95905662F28BDB46C51863102AF70C973F610F21F921D00C0B8182551845181B630C42C8A076DB70F913F61092BF12924345029146CE43441342484820A4108CB047F0A0E1F227EC21247F25A40D6906441042CA1C41500A46249944070286118861A636180776088779980737A08572C0077AA8077928073920053EB0877218077A780779E0037360877708077A600330A0033F00033FD0033D688774808779F8057AC807782807149099C4601CD8211CE6611EDC8016CA011FE8A11EE4A11CE48014F8C01ECA611CE8E11DE4810FCC811DDE211CE8810DC0800EFC000CFC00091753BE499A224A987C16609E8588D80998081410DAE94000000000131472C08774608736688779680372C0870DAF500E6DD00E7A500E6D000F7A300772A0077320076D900E71A0077320076D900E78A0077320076D900E7160077A300772D006E9300772A0077320076D900E7640077A600774D006E6100776A0077320076D600E7320077A300772D006E6600774A0077640076DE00E78A0077160077A300772A007764007439E0000000000000000000000863C06100001000000000000000C79102000040000000000000018F23440000C0000000000000030E4818000080000000000000060C84301011000000000000000401608000F000000321E981419114C908C092647C604432225500C0531025004255106E541A524CAA01046008AA04048CF00109F01A03E96C310000000CF03008140200079180000630000001A034C90460213C43120C31B4381934BB30BA32B4B018971C1718171A1B999C9014121CB09AB29CBB9218349D910041304C2982010C7066120260804B241180C0A70731B0684202608443241D02802130442992010CB0641713624CAC20CCAD028CF86009A2070D5064491184519146043306D202200A026089DB521B026080240036AAA292CCD8DCB94D517D4DB5C1A5DDA9BDB04A1702608C5B32150260805344128A20D8BA26D5CE70D9EF20144A88AB0869E9EA488260885344120980DC2188CC186650883EDEBC4601083E1230316434F4C4F521304A2D9208CC1196C581A33D8BE4E0C06AFF9D060C300066590064CA6ACBEA8C2E4CECAE82608C5B46151D6606383EE1B3CE543830D411B6C18D4C00D800D0596BD4105546163B36B7349232B73A39B120455C8F05CECCAE4E6D2DEDCA6044413323C17BB3036BB32B9298151870CCF650E2D8CAC4CAEE98DAC8C6D4A809421C373912B9B7BAB931B2B9B9B125075C8F05CECD2CAEE92C8A6E8C2E8CAA604561D323C9732373AB93CA8B73437BAB929C11B00000000791800004C0000003308801CC4E11C6614013D88433884C38C4280077978077398710CE6000FED100EF4800E330C421EC2C11DCEA11C6630053D88433884831BCC033DC8433D8C033DCC788C7470077B08077948877070077A700376788770208719CC110EEC900EE1300F6E300FE3F00EF0500E3310C41DDE211CD8211DC2611E6630893BBC833BD04339B4033CBC833C84033BCCF0147660077B680737688772680737808770908770600776280776F8057678877780875F08877118877298877998812CEEF00EEEE00EF5C00EEC300362C8A11CE4A11CCCA11CE4A11CDC611CCA211CC4811DCA6106D6904339C84339984339C84339B8C33894433888033B94C32FBC833CFC823BD4033BB0C30CC421077C70037A288776808719D1430EF8E006E4200EE7E006F6100EF2C00EE1900FEF500FF400000071200000190000000660A4AC09208D1150C3E53B8F0F348D3301131102CDB01056D00D97EF3CBE1011C0448440332CC41739CC863403D21816300D97EF3CFEE20083D83CD4E417B76D03D070F9CEE34B00F32C845FDCB60954C3E53B8F2F4D4E44A0D4F450935FDC36000000612000004A0000001304412C100000000C00000034470088CC0014422994EC40C10E94244471141E9512A03787A07473085E42720E22498C680E22499287C60C000000002306080082604085C13274CF8801028020185062C00C1E34629000200806C6193CDE774123060900826060A001F48141128D182400088281910611188401268D182400088281A10652188801378D182400088281B10693198CC1468D182400088281C106D4199041538D181C00088201A40612D28C181C00088201B40613D28C183C00088241D306911010C3B29CC11960CB6842008C2608C168C2208C261083118D7C8C68E46344231F231AF98C18240008820172071F1CC0011A1023060900826080DCC1070770900D23060900826080DCC10707707006C28841028020182077F0C1011CA841800000000000000000";

    public const string VSByLegacyHexSpirv = "030223070006010000000E00240000000000000011000200010000000E00030000000000010000000F000C00000000000100000056534D61696E00000200000003000000040000000500000006000000070000000800000003000300050000009402000005000A0009000000747970652E436F6E7374616E744275666665722E436F6E7374616E747300000006000600090000000000000050726F6A656374696F6E00000500050008000000636F6E7374616E74730000000500070002000000696E2E7661722E504F534954494F4E30000000000500070003000000696E2E7661722E544558434F4F524430000000000500060004000000696E2E7661722E434F4C4F523000000005000700060000006F75742E7661722E544558434F4F52443000000005000600070000006F75742E7661722E434F4C4F52300000050004000100000056534D61696E000047000400050000000B0000000000000047000400020000001E0000000000000047000400030000001E0000000100000047000400040000001E0000000200000047000400060000001E0000000000000047000400070000001E0000000100000047000400080000002200000000000000470004000800000021000000000000004800050009000000000000002300000000000000480005000900000000000000070000001000000048000400090000000000000005000000470003000900000002000000150004000A00000020000000010000002B0004000A0000000B00000000000000160003000C000000200000002B0004000C0000000D000000000000002B0004000C0000000E0000000000803F170004000F0000000C0000000400000018000400100000000F000000040000001E00030009000000100000002000040011000000020000000900000017000400120000000C00000002000000200004001300000001000000120000002000040014000000010000000F0000002000040015000000030000000F000000200004001600000003000000120000001300020017000000210003001800000017000000200004001900000002000000100000003B0004001100000008000000020000003B0004001300000002000000010000003B0004001300000003000000010000003B0004001400000004000000010000003B0004001500000005000000030000003B0004001600000006000000030000003B0004001500000007000000030000003600050017000000010000000000000018000000F80002001A0000003D000400120000001B000000020000003D000400120000001C000000030000003D0004000F0000001D00000004000000510005000C0000001E0000001B00000000000000510005000C0000001F0000001B00000001000000500007000F000000200000001E0000001F0000000D0000000E000000410005001900000021000000080000000B0000003D000400100000002200000021000000910005000F0000002300000022000000200000003E00030005000000230000003E000300060000001C0000003E000300070000001D000000FD00010038000100";

    public const string VSByLinearHexSpirv = "030223070006010000000E00310000000000000011000200010000000E00030000000000010000000F000C00000000000100000056534D61696E00000200000003000000040000000500000006000000070000000800000003000300050000009402000005000A0009000000747970652E436F6E7374616E744275666665722E436F6E7374616E747300000006000600090000000000000050726F6A656374696F6E00000500050008000000636F6E7374616E74730000000500070002000000696E2E7661722E504F534954494F4E30000000000500070003000000696E2E7661722E544558434F4F524430000000000500060004000000696E2E7661722E434F4C4F523000000005000700060000006F75742E7661722E544558434F4F52443000000005000600070000006F75742E7661722E434F4C4F52300000050004000100000056534D61696E000047000400050000000B0000000000000047000400020000001E0000000000000047000400030000001E0000000100000047000400040000001E0000000200000047000400060000001E0000000000000047000400070000001E0000000100000047000400080000002200000000000000470004000800000021000000000000004800050009000000000000002300000000000000480005000900000000000000070000001000000048000400090000000000000005000000470003000900000002000000150004000A00000020000000010000002B0004000A0000000B00000000000000160003000C000000200000002B0004000C0000000D000000000000002B0004000C0000000E0000000000803F2B0004000C0000000F00000012519C3E2B0004000C00000010000000C4A22E3F17000400110000000C000000030000002C00060011000000120000001000000010000000100000002B0004000C00000013000000C22C4D3C2C000600110000001400000013000000130000001300000017000400150000000C00000004000000180004001600000015000000040000001E00030009000000160000002000040017000000020000000900000017000400180000000C0000000200000020000400190000000100000018000000200004001A0000000100000015000000200004001B0000000300000015000000200004001C0000000300000018000000130002001D000000210003001E0000001D000000200004001F00000002000000160000003B0004001700000008000000020000003B0004001900000002000000010000003B0004001900000003000000010000003B0004001A00000004000000010000003B0004001B00000005000000030000003B0004001C00000006000000030000003B0004001B0000000700000003000000360005001D00000001000000000000001E000000F8000200200000003D0004001800000021000000020000003D0004001800000022000000030000003D000400150000002300000004000000510005000C000000240000002100000000000000510005000C00000025000000210000000100000050000700150000002600000024000000250000000D0000000E000000410005001F00000027000000080000000B0000003D00040016000000280000002700000091000500150000002900000028000000260000004F000800110000002A00000023000000230000000000000001000000020000008E000500110000002B0000002A0000000F00000081000500110000002C0000002B0000001200000085000500110000002D0000002A0000002C00000081000500110000002E0000002D0000001400000085000500110000002F0000002A0000002E0000004F0009001500000030000000230000002F000000040000000500000006000000030000003E00030005000000290000003E00030006000000220000003E0003000700000030000000FD00010038000100";

    public const string PSHexSpirv = "030223070006010000000E001C0000000000000011000200010000000E00030000000000010000000F000A00040000000100000050534D61696E000002000000030000000400000005000000060000001000030001000000070000000300030005000000940200000500060007000000747970652E73616D706C657200000000050005000500000073616D706C657230000000000500060008000000747970652E32642E696D61676500000005000500060000007465787475726530000000000500070002000000696E2E7661722E544558434F4F524430000000000500060003000000696E2E7661722E434F4C4F523000000005000700040000006F75742E7661722E53565F544152474554000000050004000100000050534D61696E00000500070009000000747970652E73616D706C65642E696D616765000047000400020000001E0000000000000047000400030000001E0000000100000047000400040000001E0000000000000047000400050000002200000000000000470004000500000021000000600000004700040006000000220000000100000047000400060000002100000020000000160003000A00000020000000170004000B0000000A000000040000001A00020007000000200004000C000000000000000700000019000900080000000A000000010000000200000000000000000000000100000000000000200004000D0000000000000008000000200004000E000000010000000B000000170004000F0000000A000000020000002000040010000000010000000F0000002000040011000000030000000B00000013000200120000002100030013000000120000001B00030009000000080000003B0004000C00000005000000000000003B0004000D00000006000000000000003B0004001000000002000000010000003B0004000E00000003000000010000003B0004001100000004000000030000003600050012000000010000000000000013000000F8000200140000003D0004000F00000015000000020000003D0004000B00000016000000030000003D0004000800000017000000060000003D0004000700000018000000050000005600050009000000190000001700000018000000570006000B0000001A000000190000001500000000000000850005000B0000001B000000160000001A0000003E000300040000001B000000FD00010038000100";

    public static void Get(Backend backend, ColorSpaceHandling colorSpaceHandling, out byte[] vs, out byte[] ps)
    {
        switch (backend)
        {
            case Backend.DirectX12:
                GetDxil(colorSpaceHandling, out vs, out ps);
                break;
            case Backend.Vulkan:
                GetSpirv(colorSpaceHandling, out vs, out ps);
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(backend));
        }
    }

    private static void GetDxil(ColorSpaceHandling colorSpaceHandling, out byte[] vs, out byte[] ps)
    {
        vs = colorSpaceHandling switch
        {
            ColorSpaceHandling.Legacy => Convert.FromHexString(VSByLegacyHexDxil),
            ColorSpaceHandling.Linear => Convert.FromHexString(VSByLinearHexDxil),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(colorSpaceHandling))
        };

        ps = Convert.FromHexString(PSHexDxil);
    }

    private static void GetSpirv(ColorSpaceHandling colorSpaceHandling, out byte[] vs, out byte[] ps)
    {
        vs = colorSpaceHandling switch
        {
            ColorSpaceHandling.Legacy => Convert.FromHexString(VSByLegacyHexSpirv),
            ColorSpaceHandling.Linear => Convert.FromHexString(VSByLinearHexSpirv),
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(colorSpaceHandling))
        };

        ps = Convert.FromHexString(PSHexSpirv);
    }
}
