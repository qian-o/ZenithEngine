using Silk.NET.Maths;
using ZenithEngine.Common;
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
        LayoutDesc layout = new();

        layout.Add(new(ElementFormat.Float3, ElementSemanticType.Position, 0));
        layout.Add(new(ElementFormat.Float3, ElementSemanticType.Normal, 0));
        layout.Add(new(ElementFormat.Float2, ElementSemanticType.TexCoord, 0));

        return layout;
    }

    public static void CornellBox(uint materialId,
                                  out Vertex[] vertices,
                                  out uint[] indices,
                                  out Material material)
    {
        // 0 : Left wall (red)
        // 1 : Right wall (green)
        // 2 : White surfaces (ceiling, floor, back wall, short block, tall block)
        // 3 : Light (short block)
        List<Vertex> verticesList = [];
        List<uint> indicesList = [];

        switch (materialId)
        {
            case 0:
                // Left wall (red)
                AddQuad(new(552.8f, 0.0f, 0.0f),
                        new(549.6f, 0.0f, 559.2f),
                        new(556.0f, 548.8f, 559.2f),
                        new(556.0f, 548.8f, 0.0f));
                break;
            case 1:
                // Right wall (green)
                AddQuad(new(0.0f, 0.0f, 559.2f),
                        new(0.0f, 0.0f, 0.0f),
                        new(0.0f, 548.8f, 0.0f),
                        new(0.0f, 548.8f, 559.2f));
                break;
            case 2:
                // Ceiling (white)
                AddQuad(new(556.0f, 548.8f, 0.0f),
                        new(556.0f, 548.8f, 559.2f),
                        new(0.0f, 548.8f, 559.2f),
                        new(0.0f, 548.8f, 0.0f));

                // Floor (white)
                AddQuad(new(552.8f, 0.0f, 0.0f),
                        new(0.0f, 0.0f, 0.0f),
                        new(0.0f, 0.0f, 559.2f),
                        new(549.6f, 0.0f, 559.2f));

                // Back wall (white)
                AddQuad(new(549.6f, 0.0f, 559.2f),
                        new(0.0f, 0.0f, 559.2f),
                        new(0.0f, 548.8f, 559.2f),
                        new(556.0f, 548.8f, 559.2f));

                // Short block (white)
                AddQuad(new(130.0f, 165.0f, 65.0f),
                        new(82.0f, 165.0f, 225.0f),
                        new(240.0f, 165.0f, 272.0f),
                        new(290.0f, 165.0f, 114.0f));
                AddQuad(new(290.0f, 0.0f, 114.0f),
                        new(290.0f, 165.0f, 114.0f),
                        new(240.0f, 165.0f, 272.0f),
                        new(240.0f, 0.0f, 272.0f));
                AddQuad(new(130.0f, 0.0f, 65.0f),
                        new(130.0f, 165.0f, 65.0f),
                        new(290.0f, 165.0f, 114.0f),
                        new(290.0f, 0.0f, 114.0f));
                AddQuad(new(82.0f, 0.0f, 225.0f),
                        new(82.0f, 165.0f, 225.0f),
                        new(130.0f, 165.0f, 65.0f),
                        new(130.0f, 0.0f, 65.0f));
                AddQuad(new(240.0f, 0.0f, 272.0f),
                        new(240.0f, 165.0f, 272.0f),
                        new(82.0f, 165.0f, 225.0f),
                        new(82.0f, 0.0f, 225.0f));

                // Tall block (white)
                AddQuad(new(423.0f, 330.0f, 247.0f),
                        new(265.0f, 330.0f, 296.0f),
                        new(314.0f, 330.0f, 456.0f),
                        new(472.0f, 330.0f, 406.0f));
                AddQuad(new(423.0f, 0.0f, 247.0f),
                        new(423.0f, 330.0f, 247.0f),
                        new(472.0f, 330.0f, 406.0f),
                        new(472.0f, 0.0f, 406.0f));
                AddQuad(new(472.0f, 0.0f, 406.0f),
                        new(472.0f, 330.0f, 406.0f),
                        new(314.0f, 330.0f, 456.0f),
                        new(314.0f, 0.0f, 456.0f));
                AddQuad(new(314.0f, 0.0f, 456.0f),
                        new(314.0f, 330.0f, 456.0f),
                        new(265.0f, 330.0f, 296.0f),
                        new(265.0f, 0.0f, 296.0f));
                AddQuad(new(265.0f, 0.0f, 296.0f),
                        new(265.0f, 330.0f, 296.0f),
                        new(423.0f, 330.0f, 247.0f),
                        new(423.0f, 0.0f, 247.0f));
                break;
            case 3:
                // Light (short block)
                AddQuad(new(343.0f, 548.8f, 227.0f),
                        new(343.0f, 548.8f, 332.0f),
                        new(213.0f, 548.8f, 332.0f),
                        new(213.0f, 548.8f, 227.0f));
                break;
            default:
                throw new ZenithEngineException(ExceptionHelpers.NotSupported(materialId));
        }

        vertices = [.. verticesList];
        indices = [.. indicesList];
        material = materialId switch
        {
            0 => new()
            {
                IsLight = false,
                Albedo = new(0.63f, 0.065f, 0.05f),
                Emission = new(0.0f)
            },
            1 => new()
            {
                IsLight = false,
                Albedo = new(0.14f, 0.45f, 0.091f),
                Emission = new(0.0f)
            },
            2 => new()
            {
                IsLight = false,
                Albedo = new(0.725f, 0.71f, 0.68f),
                Emission = new(0.0f)
            },
            3 => new()
            {
                IsLight = true,
                Albedo = new(0.65f),
                Emission = new(47.8348007f, 38.5663986f, 31.0807991f)
            },
            _ => throw new ZenithEngineException(ExceptionHelpers.NotSupported(materialId))
        };

        void AddQuad(Vector3D<float> v0,
                     Vector3D<float> v1,
                     Vector3D<float> v2,
                     Vector3D<float> v3)
        {
            Vector3D<float> normal = Vector3D.Normalize(Vector3D.Cross(v1 - v0, v2 - v0));

            uint startIndex = (uint)verticesList.Count;

            verticesList.Add(new(v0, normal, new(0, 0)));
            verticesList.Add(new(v1, normal, new(1, 0)));
            verticesList.Add(new(v2, normal, new(1, 1)));
            verticesList.Add(new(v3, normal, new(0, 1)));

            indicesList.Add(startIndex);
            indicesList.Add(startIndex + 1);
            indicesList.Add(startIndex + 2);
            indicesList.Add(startIndex);
            indicesList.Add(startIndex + 2);
            indicesList.Add(startIndex + 3);
        }
    }
}
