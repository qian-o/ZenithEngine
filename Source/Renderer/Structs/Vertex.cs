using System.Numerics;

namespace Renderer.Structs;

internal readonly record struct Vertex
{
    public Vertex(Vector3 position, Vector3 normal, Vector2 texCoord, Vector3 color, Vector4 tangent)
    {
        Position = position;
        Normal = normal;
        TexCoord = texCoord;
        Color = color;
        Tangent = tangent;
    }

    public Vector3 Position { get; init; }

    public Vector3 Normal { get; init; }

    public Vector2 TexCoord { get; init; }

    public Vector3 Color { get; init; }

    public Vector4 Tangent { get; init; }
}
