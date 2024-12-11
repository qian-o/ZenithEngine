using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGui;

public class ImGuiRenderer : DisposableObject
{
    public ImGuiRenderer(GraphicsContext context, OutputDesc desc, ColorSpaceHandling handling)
    {
        Context = context;

        CreateGraphicsResources(desc, handling);
    }

    public GraphicsContext Context { get; }

    protected override void Destroy()
    {
    }

    private void CreateGraphicsResources(OutputDesc desc, ColorSpaceHandling handling)
    {
    }
}
