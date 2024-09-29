namespace Graphics.Vulkan;

public static class ResourceFactoryExtensions
{
    public static Shader[] HlslToSpirvByShaderc(this ResourceFactory factory,
                                                ShaderDescription[] descriptions,
                                                Func<string, byte[]>? includeResolver = null)
    {
        Shader[] shaders = new Shader[descriptions.Length];

        for (int i = 0; i < descriptions.Length; i++)
        {
            ShaderDescription description = descriptions[i];

            byte[] spirv = ShadercHelpers.CompileHlslToSpirv(in descriptions[i], includeResolver);

            shaders[i] = factory.CreateShader(new ShaderDescription(description.Stage, spirv, description.EntryPoint));
        }

        return shaders;
    }

    public static Shader[] HlslToSpirvByShaderc(this ResourceFactory factory,
                                                params ShaderDescription[] descriptions)
    {
        return HlslToSpirvByShaderc(factory, descriptions, null);
    }

    public static Shader[] HlslToSpirvByDxc(this ResourceFactory factory,
                                            ShaderDescription[] descriptions,
                                            Func<string, byte[]>? includeResolver = null)
    {
        Shader[] shaders = new Shader[descriptions.Length];

        for (int i = 0; i < descriptions.Length; i++)
        {
            ShaderDescription description = descriptions[i];

            byte[] spirv = DxcHelpers.CompileHlslToSpirv(in descriptions[i], includeResolver);

            shaders[i] = factory.CreateShader(new ShaderDescription(description.Stage, spirv, description.EntryPoint));
        }

        return shaders;
    }
}
