using Graphics.Core;
using Graphics.Vulkan;
using SharpGLTF.Schema2;
using StbiSharp;
using GltfSampler = SharpGLTF.Schema2.TextureSampler;
using GltfTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Renderer.Models;

internal sealed class GLTF : DisposableObject
{
    protected override void Destroy()
    {
    }

    public static GLTF Load(string path)
    {
        ModelRoot root = ModelRoot.Load(path);

        List<Texture> textures = [];
        List<TextureView> textureViews = [];
        List<Sampler> samplers = [];

        CommandList commandList = App.ResourceFactory.CreateGraphicsCommandList();

        commandList.Begin();
        foreach (GltfTexture gltfTexture in root.LogicalTextures)
        {
            Stbi.InfoFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, out int width, out int height, out _);

            StbiImage image = Stbi.LoadFromMemory(gltfTexture.PrimaryImage.Content.Content.Span, 4);

            TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                          (uint)height,
                                                                          MipLevels(width, height),
                                                                          PixelFormat.R8G8B8A8UNorm,
                                                                          TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = App.ResourceFactory.CreateTexture(in description);
            TextureView textureView = App.ResourceFactory.CreateTextureView(texture);

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            image.Dispose();
        }
        commandList.End();
        App.GraphicsDevice.SubmitCommands(commandList);

        foreach (GltfSampler gltfSampler in root.LogicalTextureSamplers)
        {
        }

        return new GLTF();
    }

    private static uint MipLevels(int width, int height)
    {
        return (uint)MathF.Floor(MathF.Log2(MathF.Max(width, height))) + 1;
    }
}
