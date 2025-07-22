using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGuiWrapper;

internal unsafe class ImGuiRenderer : DisposableObject
{
    [StructLayout(LayoutKind.Explicit)]
    private struct Constants
    {
        [FieldOffset(0)]
        public Matrix4X4<float> Projection;
    }

    private readonly Dictionary<ulong, Texture> textures = [];
    private readonly Dictionary<ulong, BindingToken> bindings = [];

    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private Buffer constantsBuffer = null!;
    private Sampler sampler = null!;
    private ResourceLayout layout = null!;
    private GraphicsPipeline pipeline = null!;

    public ImGuiRenderer(GraphicsContext context,
                         OutputDesc outputDesc,
                         ColorSpaceHandling colorSpaceHandling)
    {
        Context = context;

        CreateGraphicsResources(outputDesc, colorSpaceHandling);
    }

    public GraphicsContext Context { get; }

    public void PrepareResources(CommandBuffer commandBuffer, ImDrawDataPtr drawDataPtr)
    {
        for (int i = 0; i < drawDataPtr.Textures.Size; i++)
        {
            ImTextureDataPtr imTexture = drawDataPtr.Textures[i];

            if (imTexture.Status is ImTextureStatus.WantCreate)
            {
                TextureDesc textureDesc = new((uint)imTexture.Width, (uint)imTexture.Height, format: imTexture.Format switch
                {
                    ImTextureFormat.Rgba32 => PixelFormat.R8G8B8A8UNorm,
                    ImTextureFormat.Alpha8 => PixelFormat.R8UNorm,
                    _ => throw new NotSupportedException($"Unsupported texture format: {imTexture.Format}")
                });

                Texture texture = Context.Factory.CreateTexture(in textureDesc);

                commandBuffer.UpdateTexture(texture,
                                            (nint)imTexture.Pixels,
                                            (uint)(imTexture.Width * imTexture.Height * imTexture.BytesPerPixel),
                                            new(width: (uint)imTexture.Width, height: (uint)imTexture.Height, depth: 1));

                imTexture.SetTexID(GetBinding(texture).TexID);

                textures[imTexture.TexID.Handle] = texture;

                imTexture.Status = ImTextureStatus.Ok;
            }
            else if (imTexture.Status is ImTextureStatus.WantUpdates)
            {
                if (textures.TryGetValue(imTexture.TexID.Handle, out Texture? texture))
                {
                    for (int j = 0; j < imTexture.Updates.Size; j++)
                    {
                        ImTextureRect rect = imTexture.Updates[j];

                        commandBuffer.UpdateTexture(texture,
                                                    (nint)imTexture.Pixels,
                                                    (uint)(rect.W * rect.H * imTexture.BytesPerPixel),
                                                    new(rect.X, rect.Y, width: rect.W, height: rect.H, depth: 1));
                    }

                    imTexture.Status = ImTextureStatus.Ok;
                }
            }
            else if (imTexture.Status is ImTextureStatus.WantDestroy)
            {
                if (textures.TryGetValue(imTexture.TexID.Handle, out Texture? texture))
                {
                    RemoveBinding(texture);

                    texture.Dispose();

                    textures.Remove(imTexture.TexID.Handle);
                }

                imTexture.Status = ImTextureStatus.Ok;
            }
        }

        commandBuffer.PrepareResources([.. bindings.Values.Select(static item => item.ResourceSet)]);
    }

    public void Render(CommandBuffer commandBuffer, ImDrawDataPtr drawDataPtr)
    {
        if (drawDataPtr.CmdListsCount is 0)
        {
            return;
        }

        uint totalVertexSize = (uint)(drawDataPtr.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVertexSize > vertexBuffer.Desc.SizeInBytes)
        {
            vertexBuffer.Dispose();

            BufferDesc vbDesc = new(totalVertexSize * 2,
                                    BufferUsage.VertexBuffer | BufferUsage.Dynamic);

            vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);
        }

        uint totalIndexSize = (uint)(drawDataPtr.TotalIdxCount * sizeof(ushort));
        if (totalIndexSize > indexBuffer.Desc.SizeInBytes)
        {
            indexBuffer.Dispose();

            BufferDesc ibDesc = new(totalIndexSize * 2,
                                    BufferUsage.IndexBuffer | BufferUsage.Dynamic);

            indexBuffer = Context.Factory.CreateBuffer(in ibDesc);
        }

        for (int i = 0, vertexOffset = 0, indexOffset = 0; i < drawDataPtr.CmdListsCount; i++)
        {
            ImDrawListPtr drawListPtr = drawDataPtr.CmdLists[i];

            int vertexSize = drawListPtr.VtxBuffer.Size * sizeof(ImDrawVert);
            int indexSize = drawListPtr.IdxBuffer.Size * sizeof(ushort);

            Context.UpdateBuffer(vertexBuffer,
                                 (nint)drawListPtr.VtxBuffer.Data,
                                 (uint)vertexSize,
                                 (uint)vertexOffset);

            Context.UpdateBuffer(indexBuffer,
                                 (nint)drawListPtr.IdxBuffer.Data,
                                 (uint)indexSize,
                                 (uint)indexOffset);

            vertexOffset += vertexSize;
            indexOffset += indexSize;
        }

        Constants constants = new()
        {
            Projection = Matrix4X4.CreateOrthographicOffCenter(drawDataPtr.DisplayPos.X,
                                                               drawDataPtr.DisplayPos.X + drawDataPtr.DisplaySize.X,
                                                               drawDataPtr.DisplayPos.Y + drawDataPtr.DisplaySize.Y,
                                                               drawDataPtr.DisplayPos.Y,
                                                               0,
                                                               1)
        };

        Context.UpdateBuffer(constantsBuffer, (nint)(&constants), (uint)sizeof(Constants));

        commandBuffer.BeginDebugEvent("ImGui");

        commandBuffer.SetGraphicsPipeline(pipeline);
        commandBuffer.SetVertexBuffer(0, vertexBuffer);
        commandBuffer.SetIndexBuffer(indexBuffer, IndexFormat.UInt16);

        for (int i = 0, vertexOffset = 0, indexOffset = 0; i < drawDataPtr.CmdListsCount; i++)
        {
            ImDrawListPtr drawListPtr = drawDataPtr.CmdLists[i];

            for (int j = 0; j < drawListPtr.CmdBuffer.Size; j++)
            {
                ImDrawCmd drawCmd = drawListPtr.CmdBuffer[j];

                if (drawCmd.UserCallback is not null)
                {
                    ImDrawCallback callback = Marshal.GetDelegateForFunctionPointer<ImDrawCallback>((nint)drawCmd.UserCallback);

                    callback(drawListPtr, &drawCmd);
                }
                else
                {
                    Vector2D<int> offset = new((int)Math.Max(0, drawCmd.ClipRect.X),
                                               (int)Math.Max(0, drawCmd.ClipRect.Y));

                    Vector2D<uint> extent = new((uint)Math.Max(0, drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                                                 (uint)Math.Max(0, drawCmd.ClipRect.W - drawCmd.ClipRect.Y));

                    if (extent.X is 0 || extent.Y is 0)
                    {
                        continue;
                    }

                    commandBuffer.SetScissorRectangles([offset], [extent]);

                    commandBuffer.SetResourceSet(0, bindings[drawCmd.TexRef.GetTexID().Handle].ResourceSet);

                    commandBuffer.DrawIndexed(drawCmd.ElemCount,
                                              1,
                                              firstIndex: (uint)(drawCmd.IdxOffset + indexOffset),
                                              vertexOffset: (int)(drawCmd.VtxOffset + vertexOffset));
                }
            }

            vertexOffset += drawListPtr.VtxBuffer.Size;
            indexOffset += drawListPtr.IdxBuffer.Size;
        }

        commandBuffer.EndDebugEvent();
    }

    public ImTextureRef GetBinding(Texture texture)
    {
        foreach (KeyValuePair<ulong, BindingToken> item in bindings)
        {
            if (item.Value.Texture == texture)
            {
                return new(texId: new(item.Key));
            }
        }

        ulong id = 0;
        while (bindings.ContainsKey(id))
        {
            id++;
        }

        ResourceSetDesc desc = new(layout, constantsBuffer, texture, sampler);

        bindings[id] = new(texture, Context.Factory.CreateResourceSet(in desc));

        return new(texId: new(id));
    }

    public void RemoveBinding(Texture texture)
    {
        foreach (KeyValuePair<ulong, BindingToken> item in bindings)
        {
            if (item.Value.Texture == texture)
            {
                item.Value.ResourceSet.Dispose();

                bindings.Remove(item.Key);

                break;
            }
        }
    }

    protected override void Destroy()
    {
        foreach (BindingToken token in bindings.Values)
        {
            token.ResourceSet.Dispose();
        }

        foreach (Texture texture in textures.Values)
        {
            texture.Dispose();
        }

        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        constantsBuffer.Dispose();
        sampler.Dispose();
        layout.Dispose();
        pipeline.Dispose();
    }

    private void CreateGraphicsResources(OutputDesc outputDesc, ColorSpaceHandling colorSpaceHandling)
    {
        BufferDesc vbDesc = new((uint)(5000 * sizeof(ImDrawVert)),
                                BufferUsage.VertexBuffer | BufferUsage.Dynamic);

        BufferDesc ibDesc = new(10000 * sizeof(ushort),
                                BufferUsage.IndexBuffer | BufferUsage.Dynamic);

        BufferDesc cbDesc = new((uint)sizeof(Constants),
                                BufferUsage.ConstantBuffer | BufferUsage.Dynamic);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);
        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);
        constantsBuffer = Context.Factory.CreateBuffer(in cbDesc);
        sampler = Context.Factory.CreateSampler(in Samplers.PointClamp);

        Shaders.Get(Context.Backend, colorSpaceHandling, out ResourceLayoutDesc layoutDesc, out byte[] vs, out byte[] ps);

        layout = Context.Factory.CreateResourceLayout(in layoutDesc);

        ShaderDesc vsShaderDesc = new(ShaderStages.Vertex, vs, Shaders.VSMain);
        ShaderDesc psShaderDesc = new(ShaderStages.Pixel, ps, Shaders.PSMain);

        using Shader vsShader = Context.Factory.CreateShader(in vsShaderDesc);
        using Shader psShader = Context.Factory.CreateShader(in psShaderDesc);

        InputLayoutDesc inputLayout = new();
        inputLayout.Add(new(ElementFormat.Float2, ElementSemanticType.Position, 0));
        inputLayout.Add(new(ElementFormat.Float2, ElementSemanticType.TexCoord, 0));
        inputLayout.Add(new(ElementFormat.UByte4Normalized, ElementSemanticType.Color, 0));

        GraphicsPipelineDesc pipelineDesc = new
        (
            shaders: new(vertex: vsShader, pixel: psShader),
            inputLayouts: [inputLayout],
            resourceLayouts: [layout],
            outputs: outputDesc,
            renderStates: new(RasterizerStates.None, DepthStencilStates.None, BlendStates.AlphaBlend)
        );

        pipeline = Context.Factory.CreateGraphicsPipeline(in pipelineDesc);
    }
}
