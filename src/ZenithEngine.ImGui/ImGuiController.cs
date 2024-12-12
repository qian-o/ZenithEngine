using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ImGui.Interfaces;

namespace ZenithEngine.ImGui;

public unsafe class ImGuiController : DisposableObject
{
    public ImGuiContextPtr ImGuiContext;

    private readonly IInputController controller;
    private readonly ImGuiRenderer renderer;

    private bool frameBegun;

    public ImGuiController(GraphicsContext context,
                           OutputDesc outputDesc,
                           IInputController inputController,
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

        controller = inputController;
        renderer = new(context, outputDesc, handling);
    }

    public void Update(double deltaSeconds)
    {
        if (frameBegun)
        {
            ImGuiApi.Render();
        }

        ImGuiApi.SetCurrentContext(ImGuiContext);

        ImGuiIOPtr io = ImGuiApi.GetIO();

        io.DeltaTime = (float)deltaSeconds;
        io.DisplaySize = controller.Size.As<float>().ToSystem();

        io.AddMousePosEvent(controller.MousePosition.X, controller.MousePosition.Y);
        io.AddMouseWheelEvent(controller.MouseWheel.X, controller.MouseWheel.Y);

        io.AddMouseButtonEvent((int)ImGuiMouseButton.Left, controller.MousePressed(ImGuiMouseButton.Left));
        io.AddMouseButtonEvent((int)ImGuiMouseButton.Right, controller.MousePressed(ImGuiMouseButton.Right));
        io.AddMouseButtonEvent((int)ImGuiMouseButton.Middle, controller.MousePressed(ImGuiMouseButton.Middle));
        io.AddInputCharactersUTF8(controller.InputText);

        ImGuiApi.NewFrame();

        ImGuiApi.DockSpaceOverViewport();

        frameBegun = true;
    }

    public void PrepareResources(CommandBuffer commandBuffer)
    {
        renderer.PrepareResources(commandBuffer);
    }

    public void Render(CommandBuffer commandBuffer)
    {
        if (frameBegun)
        {
            ImGuiApi.Render();

            renderer.Render(commandBuffer, ImGuiApi.GetDrawData());

            frameBegun = false;
        }
    }

    protected override void Destroy()
    {
        renderer.Dispose();

        ImGuiApi.SetCurrentContext(null);

        ImGuiApi.DestroyContext(ImGuiContext);
    }
}
