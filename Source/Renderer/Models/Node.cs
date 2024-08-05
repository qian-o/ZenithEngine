using System.Numerics;
using Renderer.Structs;

namespace Renderer.Models;

internal sealed class Node(string name, Node? parent, Node[] children, Primitive[] primitives, Matrix4x4 localTransform)
{
    public string Name { get; } = name;

    public Node? Parent { get; } = parent;

    public Node[] Children { get; } = children;

    public Primitive[] Primitives { get; } = primitives;

    public Matrix4x4 LocalTransform { get; } = localTransform;
}
