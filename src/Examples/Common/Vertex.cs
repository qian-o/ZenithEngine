using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace Common;

public struct Vertex(Vector3D<float> position, Vector3D<float> normal, Vector2D<float> texCoord)
{
    public Vector3D<float> Position = position;

    public Vector3D<float> Normal = normal;

    public Vector2D<float> TexCoord = texCoord;

    public static LayoutDesc GetLayout()
    {
        LayoutDesc layout = LayoutDesc.New();

        layout.Add(new(ElementFormat.Float3, ElementSemanticType.Position, 0));
        layout.Add(new(ElementFormat.Float3, ElementSemanticType.Normal, 0));
        layout.Add(new(ElementFormat.Float2, ElementSemanticType.TexCoord, 0));

        return layout;
    }
}
