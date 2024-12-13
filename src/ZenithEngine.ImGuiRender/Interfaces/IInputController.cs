using Hexa.NET.ImGui;
using Silk.NET.Maths;

namespace ZenithEngine.ImGuiRender.Interfaces;

public interface IInputController
{
    Vector2D<uint> Size { get; }

    Vector2D<int> MousePosition { get; }

    Vector2D<int> MouseWheel { get; }

    string InputText { get; }

    bool MousePressed(ImGuiMouseButton button);

    bool KeyPressed(ImGuiKey key);
}
