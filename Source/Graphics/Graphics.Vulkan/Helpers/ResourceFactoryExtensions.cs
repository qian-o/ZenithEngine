namespace Graphics.Vulkan;

public static class ResourceFactoryExtensions
{
    public static Shader[] HlslToSpirv(this ResourceFactory factory,
                                       ShaderDescription[] descriptions,
                                       Func<string, byte[]>? includeResolver = null)
    {
        Shader[] shaders = new Shader[descriptions.Length];

        for (int i = 0; i < descriptions.Length; i++)
        {
            ShaderDescription description = descriptions[i];

            byte[] spirv = DxcHelpers.Compile(in descriptions[i], includeResolver);

            shaders[i] = factory.CreateShader(new ShaderDescription(description.Stage, spirv, description.EntryPoint));
        }

        return shaders;
    }

    public static Shader[] HlslToSpirv(this ResourceFactory factory, params ShaderDescription[] descriptions)
    {
        return HlslToSpirv(factory, descriptions, null);
    }
}
