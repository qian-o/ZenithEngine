namespace Graphics.Vulkan;

public static class ResourceFactoryExtensions
{
    public static Shader[] CreateFromSpirv(this ResourceFactory factory, params ShaderDescription[] descriptions)
    {
        Shader[] shaders = new Shader[descriptions.Length];

        for (int i = 0; i < descriptions.Length; i++)
        {
            ShaderDescription description = descriptions[i];

            byte[] spirv = SpirvCompilation.CompileGlslToSpirv(in descriptions[i]);

            shaders[i] = factory.CreateShader(new ShaderDescription(description.Stage, spirv, description.EntryPoint));
        }

        return shaders;
    }
}
