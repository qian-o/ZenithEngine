using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;
using ZenithEngine.Common.Graphics;
using ZenithEngine.ImGui;
using ZenithEngine.ImGui.Interfaces;
using ZenithEngine.Windowing;
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

            window.MouseMove += Window_MouseMove;
            window.MouseWheel += Window_MouseWheel;
            window.KeyChar += Window_KeyChar;
        }

        public Vector2D<uint> Size => window.Size;

        public Vector2D<int> MousePosition { get; private set; }

        public Vector2D<int> MouseWheel { get; private set; }

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
            return button switch
            {
                ImGuiMouseButton.Left => WindowUtils.IsMouseButtonDown(MouseButton.Left),
                ImGuiMouseButton.Right => WindowUtils.IsMouseButtonDown(MouseButton.Right),
                ImGuiMouseButton.Middle => WindowUtils.IsMouseButtonDown(MouseButton.Middle),
                ImGuiMouseButton.Count => WindowUtils.IsMouseButtonDown(MouseButton.Button4),
                _ => false,
            };
        }

        private void Window_MouseMove(object? sender, ValueEventArgs<Vector2D<int>> e)
        {
            MousePosition = e.Value;
        }

        private void Window_MouseWheel(object? sender, ValueEventArgs<Vector2D<int>> e)
        {
            MouseWheel = e.Value;
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
