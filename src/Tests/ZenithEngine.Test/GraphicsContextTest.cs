using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Test;

[TestClass]
public class GraphicsContextTest
{
    public const Backend RenderBackend = Backend.Vulkan;

    [TestMethod]
    public void TestCreateDevice()
    {
        try
        {
            using GraphicsContext context = GraphicsContext.Create(RenderBackend);

            context.CreateDevice(true);
        }
        catch (Exception)
        {
            Assert.Fail();
        }
    }

    [TestMethod]
    public void TestShader()
    {
        using GraphicsContext context = GraphicsContext.Create(RenderBackend);

        context.CreateDevice(true);
    }
}
