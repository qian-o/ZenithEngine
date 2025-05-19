import "Common/Random";
import "Common/Structs";
import "Uniforms";

[shader("closesthit")]
void Main(inout Payload payload, BuiltInTriangleIntersectionAttributes attrib)
{
    Vertex vertex = Vertex.GetVertex(vertices, indices, offsets, attrib);

    payload.Normal = vertex.Normal;
}