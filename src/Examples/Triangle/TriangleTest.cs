using Common;
using Hexa.NET.ImGui;
using ZenithEngine.Common.Enums;

namespace Triangle;

internal class TriangleTest(Backend backend) : VisualTest("Triangle Test", backend)
{
    protected override void OnLoad()
    {
    }

    protected override void OnUpdate(double deltaTime, double totalTime)
    {
    }

    protected override void OnRender(double deltaTime, double totalTime)
    {
        ImGui.ShowDemoWindow();
    }
}
