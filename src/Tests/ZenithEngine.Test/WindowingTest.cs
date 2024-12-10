using ZenithEngine.Windowing;
using ZenithEngine.Windowing.Interfaces;

namespace ZenithEngine.Test;

[TestClass]
public class WindowingTest
{
    [TestMethod]
    public void TestCreateWindow()
    {
        IWindow window = WindowController.CreateWindow();

        window.Show();

        WindowController.Loop();
    }
}
