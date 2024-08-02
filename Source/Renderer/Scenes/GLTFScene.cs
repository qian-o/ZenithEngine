using Graphics.Core;
using Graphics.Vulkan;
using SharpGLTF.Schema2;
using Scene = Renderer.Components.Scene;

namespace Renderer.Scenes;

internal sealed class GLTFScene(MainWindow mainWindow) : Scene(mainWindow)
{
    protected override void Initialize()
    {
        Title = "GLTF Scene";

        ModelRoot root = ModelRoot.Load("Assets/Models/Sponza/glTF/Sponza.gltf");

        foreach (Mesh mesh in root.LogicalMeshes)
        {
            Console.WriteLine(mesh.Name);
        }
    }

    protected override void RecreatePipeline(Framebuffer framebuffer)
    {
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
    }

    protected override void RenderCore(CommandList commandList, Framebuffer framebuffer, RenderEventArgs e)
    {
    }
}
