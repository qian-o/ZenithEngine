namespace ZenithEngine.Windowing.Interfaces;

public interface IWindow : IWindowEvents, IWindowProperties
{
    void Show();

    void Close();

    void Focus();

    void DoEvents();

    void DoUpdate();

    void DoRender();
}
