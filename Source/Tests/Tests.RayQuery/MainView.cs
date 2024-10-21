using System.Numerics;
using System.Runtime.InteropServices;
using Graphics.Core;
using Graphics.Core.Window;
using Graphics.Vulkan;
using Graphics.Vulkan.Descriptions;
using Graphics.Vulkan.ImGui;
using SharpGLTF.Schema2;
using SharpGLTF.Validation;
using StbImageSharp;
using Tests.Core;
using Tests.Core.Helpers;
using GLTFTexture = SharpGLTF.Schema2.Texture;
using Texture = Graphics.Vulkan.Texture;

namespace Tests.RayQuery;

internal sealed unsafe class MainView : View
{
    #region Structs
    [StructLayout(LayoutKind.Sequential)]
    private struct Vertex
    {
        public Vector3 Position;

        public Vector3 Normal;

        public Vector2 TexCoord;

        public Vector3 Color;

        public Vector4 Tangent;

        public uint NodeIndex;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GeometryNode
    {
        public bool AlphaMask;

        public float AlphaCutoff;

        public bool DoubleSided;

        public Vector4 BaseColorFactor;

        public uint BaseColorTextureIndex;

        public uint NormalTextureIndex;

        public uint RoughnessTextureIndex;
    }
    #endregion

    private readonly GraphicsDevice _device;
    private readonly ImGuiController _imGuiController;
    private readonly ViewController _viewController;
    private readonly CameraController _cameraController;

    private readonly List<Texture> _textures;
    private readonly List<TextureView> _textureViews;
    private readonly List<Vertex> _vertices;
    private readonly List<uint> _indices;
    private readonly List<GeometryNode> _nodes;

    private static DeviceBuffer _vertexBuffer = null!;
    private static DeviceBuffer _indexBuffer = null!;
    private static DeviceBuffer _nodeBuffer = null!;

    public MainView(GraphicsDevice device, ImGuiController imGuiController) : base("Ray Query")
    {
        _device = device;
        _imGuiController = imGuiController;
        _viewController = new ViewController(this);
        _cameraController = new CameraController(_viewController);
        _cameraController.Transform(Matrix4x4.CreateRotationY(90.0f.ToRadians()) * Matrix4x4.CreateTranslation(new Vector3(0.0f, 1.2f, 0.0f)));
        
        _textures = [];
        _textureViews = [];
        _vertices = [];
        _indices = [];
        _nodes = [];

        LoadGLTF("Assets/Models/Sponza/glTF/Sponza.gltf");

        _vertexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<Vertex>(_vertices.Count, BufferUsage.VertexBuffer));
        _device.UpdateBuffer(_vertexBuffer, [.. _vertices]);

        _indexBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<uint>(_indices.Count, BufferUsage.IndexBuffer));
        _device.UpdateBuffer(_indexBuffer, [.. _indices]);

        _nodeBuffer = _device.Factory.CreateBuffer(BufferDescription.Buffer<GeometryNode>(_nodes.Count, BufferUsage.StorageBuffer));
        _device.UpdateBuffer(_nodeBuffer, [.. _nodes]);
    }

    protected override void OnUpdate(UpdateEventArgs e)
    {
    }

    protected override void OnRender(RenderEventArgs e)
    {
    }

    protected override void OnResize(ResizeEventArgs e)
    {
    }

    protected override void Destroy()
    {
    }

    private void LoadGLTF(string path)
    {
        ModelRoot root = ModelRoot.Load(path, new ReadSettings() { Validation = ValidationMode.Skip });

        #region Load Textures
        using CommandList commandList = _device.Factory.CreateGraphicsCommandList();

        commandList.Begin();

        foreach (GLTFTexture gltfTexture in root.LogicalTextures)
        {
            using Stream stream = gltfTexture.PrimaryImage.Content.Open();

            if (ImageInfo.FromStream(stream) is not ImageInfo imageInfo)
            {
                continue;
            }

            int width = imageInfo.Width;
            int height = imageInfo.Height;

            ImageResult image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

            uint mipLevels = Math.Max(1, (uint)MathF.Log2(Math.Max(width, height))) + 1;

            TextureDescription description = TextureDescription.Texture2D((uint)width,
                                                                          (uint)height,
                                                                          mipLevels,
                                                                          PixelFormat.R8G8B8A8UNorm,
                                                                          TextureUsage.Sampled | TextureUsage.GenerateMipmaps);

            Texture texture = _device.Factory.CreateTexture(in description);
            texture.Name = gltfTexture.Name;

            TextureView textureView = _device.Factory.CreateTextureView(texture);
            textureView.Name = gltfTexture.Name;

            commandList.UpdateTexture(texture, image.Data, 0, 0, 0, (uint)width, (uint)height, 1, 0, 0);
            commandList.GenerateMipmaps(texture);

            _textures.Add(texture);
            _textureViews.Add(textureView);

            gltfTexture.ClearImages();
        }

        commandList.End();

        _device.SubmitCommands(commandList);
        #endregion

    }
}
