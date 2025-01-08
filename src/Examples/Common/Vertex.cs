using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace Common;

public struct Vertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoord)
{
    public readonly static LayoutDesc Layout;

    static Vertex()
    {
        Layout = LayoutDesc.Default();

        Layout.Add(ElementDesc.Default(ElementFormat.Float3, ElementSemanticType.Position, 0));
        Layout.Add(ElementDesc.Default(ElementFormat.Float3, ElementSemanticType.Normal, 0));
        Layout.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.TexCoord, 0));
    }

    public Vector3D<float> Position = position;

    public Vector3D<float> Normal = normal;

    public Vector2D<float> TexCoord = texCoord;
}
