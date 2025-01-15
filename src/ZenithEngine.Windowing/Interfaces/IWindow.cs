using ZenithEngine.Common.Interfaces;

namespace ZenithEngine.Windowing.Interfaces;

public interface IWindow : IWindowEvents, IWindowProperties, IInputController
{
    void Show();

    void Close();

    void Center();

    void Focus();

    void DoEvents();

    void DoUpdate();

    void DoRender();
}
