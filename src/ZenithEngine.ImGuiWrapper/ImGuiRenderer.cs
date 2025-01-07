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

    private readonly Dictionary<ulong, BindingToken> bindings = [];

    private Texture? fontTexture;

    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private Buffer constantsBuffer = null!;
    private Sampler sampler = null!;
    private ResourceLayout layout0 = null!;
    private ResourceLayout layout1 = null!;
    private ResourceSet set0 = null!;
    private GraphicsPipeline pipeline = null!;

    public ImGuiRenderer(GraphicsContext context,
                         OutputDesc outputDesc,
                         ColorSpaceHandling colorSpaceHandling)
    {
        Context = context;

        CreateGraphicsResources(outputDesc, colorSpaceHandling);
    }

    public GraphicsContext Context { get; }

    public void CreateFontDeviceTexture()
    {
        if (fontTexture is not null)
        {
            RemoveBinding(fontTexture);

            fontTexture.Dispose();
        }

        byte* pixels;
        int width;
        int height;
        ImGui.GetIO().Fonts.GetTexDataAsRGBA32(&pixels, &width, &height);

        TextureDesc fontTextureDesc = TextureDesc.Default((uint)width, (uint)height, 1, 1);

        fontTexture = Context.Factory.CreateTexture(in fontTextureDesc);

        Context.UpdateTexture(fontTexture,
                              (nint)pixels,
                              (uint)(width * height * 4),
                              TextureRegion.Default((uint)width, (uint)height, 1));

        GetBinding(fontTexture);
    }

    public void PrepareResources(CommandBuffer commandBuffer)
    {
        foreach (BindingToken token in bindings.Values)
        {
            commandBuffer.PrepareResources(token.ResourceSet);
        }
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

            BufferDesc vbDesc = BufferDesc.Default(totalVertexSize * 2,
                                                   BufferUsage.VertexBuffer | BufferUsage.Dynamic);

            vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);
        }

        uint totalIndexSize = (uint)(drawDataPtr.TotalIdxCount * sizeof(ushort));
        if (totalIndexSize > indexBuffer.Desc.SizeInBytes)
        {
            indexBuffer.Dispose();

            BufferDesc ibDesc = BufferDesc.Default(totalIndexSize * 2,
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
                                                               0.0f,
                                                               1.0f)
        };

        Context.UpdateBuffer(constantsBuffer, (nint)(&constants), (uint)sizeof(Constants));

        commandBuffer.SetGraphicsPipeline(pipeline);
        commandBuffer.SetVertexBuffer(0, vertexBuffer);
        commandBuffer.SetIndexBuffer(indexBuffer, IndexFormat.U16Bit);
        commandBuffer.SetResourceSet(0, set0);

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
                    Rectangle<int> scissor = new((int)Math.Max(0, drawCmd.ClipRect.X),
                                                 (int)Math.Max(0, drawCmd.ClipRect.Y),
                                                 (int)Math.Max(0, drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                                                 (int)Math.Max(0, drawCmd.ClipRect.W - drawCmd.ClipRect.Y));

                    if (scissor.Size.X is 0 || scissor.Size.Y is 0)
                    {
                        continue;
                    }

                    commandBuffer.SetScissorRectangles([scissor]);

                    commandBuffer.SetResourceSet(1, bindings[drawCmd.TextureId.Handle].ResourceSet);

                    commandBuffer.DrawIndexed(drawCmd.ElemCount,
                                              (uint)(drawCmd.IdxOffset + indexOffset),
                                              (uint)(drawCmd.VtxOffset + vertexOffset));
                }
            }

            vertexOffset += drawListPtr.VtxBuffer.Size;
            indexOffset += drawListPtr.IdxBuffer.Size;
        }
    }

    public ulong GetBinding(Texture texture)
    {
        foreach (KeyValuePair<ulong, BindingToken> item in bindings)
        {
            if (item.Value.Texture == texture)
            {
                return item.Key;
            }
        }

        ulong id = 0;
        while (true)
        {
            if (!bindings.ContainsKey(id))
            {
                break;
            }

            id++;
        }

        ResourceSetDesc desc = ResourceSetDesc.Default(layout1, texture);

        bindings[id] = new(texture, Context.Factory.CreateResourceSet(in desc));

        return id;
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

        fontTexture?.Dispose();

        vertexBuffer.Dispose();
        indexBuffer.Dispose();
        constantsBuffer.Dispose();
        sampler.Dispose();
        layout0.Dispose();
        layout1.Dispose();
        set0.Dispose();
        pipeline.Dispose();
    }

    private void CreateGraphicsResources(OutputDesc outputDesc, ColorSpaceHandling colorSpaceHandling)
    {
        BufferDesc vbDesc = BufferDesc.Default((uint)sizeof(ImDrawVert) * 5000,
                                               BufferUsage.VertexBuffer | BufferUsage.Dynamic);

        BufferDesc ibDesc = BufferDesc.Default(sizeof(ushort) * 10000,
                                               BufferUsage.IndexBuffer | BufferUsage.Dynamic);

        BufferDesc cbDesc = BufferDesc.Default((uint)sizeof(Constants),
                                               BufferUsage.ConstantBuffer | BufferUsage.Dynamic);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);
        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);
        constantsBuffer = Context.Factory.CreateBuffer(in cbDesc);
        sampler = Context.Factory.CreateSampler(in Samplers.PointClamp);

        ResourceLayoutDesc layout0Desc = ResourceLayoutDesc.Default(
        [
            LayoutElementDesc.Default(ShaderStages.Vertex, ResourceType.ConstantBuffer, 0),
            LayoutElementDesc.Default(ShaderStages.Pixel, ResourceType.Sampler, 0)
        ]);

        ResourceLayoutDesc layout1Desc = ResourceLayoutDesc.Default(
        [
            LayoutElementDesc.Default(ShaderStages.Pixel, ResourceType.Texture, 0)
        ]);

        layout0 = Context.Factory.CreateResourceLayout(in layout0Desc);
        layout1 = Context.Factory.CreateResourceLayout(in layout1Desc);

        ResourceSetDesc set0Desc = ResourceSetDesc.Default(layout0, constantsBuffer, sampler);

        set0 = Context.Factory.CreateResourceSet(in set0Desc);

        Shaders.Get(colorSpaceHandling, out byte[] vs, out byte[] ps);
        ShaderDesc vsShaderDesc = ShaderDesc.Default(ShaderStages.Vertex, vs, Shaders.VSMain);
        ShaderDesc psShaderDesc = ShaderDesc.Default(ShaderStages.Pixel, ps, Shaders.PSMain);

        using Shader vsShader = Context.Factory.CreateShader(in vsShaderDesc);
        using Shader psShader = Context.Factory.CreateShader(in psShaderDesc);

        LayoutDesc layoutDesc = LayoutDesc.Default();
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.Position, 0));
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.TexCoord, 0));
        layoutDesc.Add(ElementDesc.Default(ElementFormat.UByte4Normalized, ElementSemanticType.Color, 0));

        GraphicsPipelineDesc pipelineDesc = GraphicsPipelineDesc.Default
        (
            shaders: GraphicsShaderDesc.Default(vertex: vsShader, pixel: psShader),
            inputLayouts: [layoutDesc],
            resourceLayouts: [layout0, layout1],
            outputs: outputDesc,
            renderStates: RenderStateDesc.Default(RasterizerStates.None, DepthStencilStates.None, BlendStates.AlphaBlend)
        );

        pipeline = Context.Factory.CreateGraphicsPipeline(in pipelineDesc);
    }
}
