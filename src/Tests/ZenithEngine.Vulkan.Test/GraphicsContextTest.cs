using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Vulkan.Test;

[TestClass]
public sealed class GraphicsContextTest
{
    [TestMethod]
    public void TestCreateDevice()
    {
		try
        {
            using GraphicsContext context = GraphicsContext.Create(Backend.Vulkan);

            context.CreateDevice(true);
        }
		catch (Exception)
		{
            Assert.Fail();
        }
    }
}
