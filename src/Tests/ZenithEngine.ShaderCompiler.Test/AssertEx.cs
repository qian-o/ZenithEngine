using ZenithEngine.Common.Enums;

namespace ZenithEngine.ShaderCompiler.Test;

internal class AssertEx
{
    public static void AreEqual(ShaderStages stages,
                                ResourceType type,
                                uint slot,
                                uint space,
                                string name,
                                uint count,
                                ReflectResource resource)
    {
        Assert.AreEqual(stages, resource.Stages);
        Assert.AreEqual(type, resource.Type);
        Assert.AreEqual(slot, resource.Slot);
        Assert.AreEqual(space, resource.Space);
        Assert.AreEqual(name, resource.Name);
        Assert.AreEqual(count, resource.Count);
    }
}
