using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ImGui;
using ZenithEngine.ImGui.Interfaces;
using ZenithEngine.Windowing;
using ZenithEngine.Windowing.Events;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Test;

[TestClass]
public class WindowingTest
{
    public const Backend RenderBackend = Backend.Vulkan;

    private class InputController : IInputController
    {
        private readonly IWindow window;
        private readonly List<char> pressedChars = [];

        public InputController(IWindow window)
        {
            this.window = window;

            window.KeyChar += Window_KeyChar;
        }

        public Vector2D<uint> Size => window.Size;

        public Vector2D<int> MousePosition => window.Position;

        public Vector2D<int> MouseWheel { get; }

        public string InputText
        {
            get
            {
                string inputText = new([.. pressedChars]);

                pressedChars.Clear();

                return inputText;
            }
        }

        public bool KeyPressed(ImGuiKey key)
        {
            return false;
        }

        public bool MousePressed(ImGuiMouseButton button)
        {
            return false;
        }

        private void Window_KeyChar(object? sender, ValueEventArgs<char> e)
        {
            pressedChars.Add(e.Value);
        }
    }

    [TestMethod]
    public void TestCreateWindow()
    {
        AssertEx.IsConsoleErrorEmpty(() =>
        {
            IWindow window = WindowController.CreateWindow();

            window.Show();

            using GraphicsContext context = GraphicsContext.Create(RenderBackend);

            context.CreateDevice(true);

            using CommandProcessor commandProcessor = context.Factory.CreateCommandProcessor();

            SwapChainDesc swapChainDesc = SwapChainDesc.Default(window.Surface);

            using SwapChain swapChain = context.Factory.CreateSwapChain(in swapChainDesc);

            using ImGuiController imguiController = new(context,
                                                        swapChain.FrameBuffer.Output,
                                                        new InputController(window));

            window.Update += (sender, e) =>
            {
                imguiController.Update(e.DeltaTime);
            };

            window.Render += (sender, e) =>
            {
                Hexa.NET.ImGui.ImGui.ShowDemoWindow();

                CommandBuffer commandBuffer = commandProcessor.CommandBuffer();

                commandBuffer.Begin();

                imguiController.PrepareResources(commandBuffer);

                commandBuffer.BeginRendering(swapChain.FrameBuffer, new ClearValue(1, new(1)));

                imguiController.Render(commandBuffer);

                commandBuffer.EndRendering();
                commandBuffer.End();

                commandBuffer.Commit();

                commandProcessor.Submit();
                commandProcessor.WaitIdle();

                swapChain.Present();
            };

            window.SizeChanged += (sender, e) =>
            {
                swapChain.Resize();
            };

            WindowController.Loop(true);
        });
    }
}
