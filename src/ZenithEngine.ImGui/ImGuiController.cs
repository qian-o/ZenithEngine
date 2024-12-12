using Hexa.NET.ImGui;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGui;

public unsafe class ImGuiController : DisposableObject
{
    private readonly ImGuiRenderer imGuiRenderer;

    public ImGuiContextPtr ImGuiContext;

    public ImGuiController(GraphicsContext context,
                           OutputDesc outputDesc,
                           ColorSpaceHandling handling = ColorSpaceHandling.Legacy,
                           ImGuiFontConfig? fontConfig = null)
    {
        ImGuiContext = ImGuiApi.CreateContext();

        ImGuiApi.SetCurrentContext(ImGuiContext);

        ImGuiIOPtr io = ImGuiApi.GetIO();

        io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
        io.ConfigFlags |= ImGuiConfigFlags.DockingEnable;

        io.BackendFlags |= ImGuiBackendFlags.HasMouseCursors;
        io.BackendFlags |= ImGuiBackendFlags.HasSetMousePos;
        io.BackendFlags |= ImGuiBackendFlags.RendererHasVtxOffset;

        if (fontConfig is not null)
        {
            io.Fonts.Clear();

            io.Fonts.AddFontFromFileTTF(fontConfig.Value.Font,
                                        (int)fontConfig.Value.Size,
                                        null,
                                        (uint*)fontConfig.Value.GlyphRange(io));
        }

        imGuiRenderer = new(context, outputDesc, handling);
    }

    protected override void Destroy()
    {
        imGuiRenderer.Dispose();

        ImGuiApi.SetCurrentContext(null);

        ImGuiApi.DestroyContext(ImGuiContext);
    }
}
