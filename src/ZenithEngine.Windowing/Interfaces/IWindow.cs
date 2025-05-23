using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Windowing.Interfaces;

public interface IWindow : IWindowEvents, IWindowProperties, IInput
{
    void Show();

    void Close();

    void Center();

    void Focus();

    void DoEvents();

    void DoUpdate();

    void DoRender();
}
