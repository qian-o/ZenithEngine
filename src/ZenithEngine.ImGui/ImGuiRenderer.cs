using System.Numerics;
using System.Runtime.InteropServices;
using Hexa.NET.ImGui;
using Silk.NET.Maths;
using ZenithEngine.Common;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.ImGui;

public unsafe class ImGuiRenderer : DisposableObject
{
    private Buffer vertexBuffer = null!;
    private Buffer indexBuffer = null!;
    private Buffer constantsBuffer = null!;
    private Sampler sampler = null!;
    private ResourceLayout layout0 = null!;
    private ResourceLayout layout1 = null!;
    private GraphicsPipeline pipeline = null!;

    private ResourceSet set0 = null!;

    private Texture fontTexture = null!;
    private TextureView fontTextureView = null!;

    private ResourceSet set1 = null!;

    public ImGuiRenderer(GraphicsContext context, OutputDesc outputDesc, ColorSpaceHandling handling)
    {
        Context = context;

        CreateGraphicsResources(outputDesc, handling);
    }

    public GraphicsContext Context { get; }

    public void PrepareResources(CommandBuffer commandBuffer)
    {
        commandBuffer.PrepareResources(set1);
    }

    public void Render(CommandBuffer commandBuffer, ImDrawDataPtr dataPtr)
    {
        if (dataPtr.CmdListsCount is 0)
        {
            return;
        }

        uint totalVertexSize = (uint)(dataPtr.TotalVtxCount * sizeof(ImDrawVert));
        if (totalVertexSize > vertexBuffer.Desc.SizeInBytes)
        {
            vertexBuffer.Dispose();

            BufferDesc vbDesc = BufferDesc.Default((uint)(totalVertexSize * 1.5),
                                                   BufferUsage.VertexBuffer | BufferUsage.Dynamic);

            vertexBuffer = Context.Factory.CreateBuffer(ref vbDesc);
        }

        uint totalIndexSize = (uint)(dataPtr.TotalIdxCount * sizeof(ushort));
        if (totalIndexSize > indexBuffer.Desc.SizeInBytes)
        {
            indexBuffer.Dispose();
            BufferDesc ibDesc = BufferDesc.Default((uint)(totalIndexSize * 1.5),
                                                   BufferUsage.IndexBuffer | BufferUsage.Dynamic);

            indexBuffer = Context.Factory.CreateBuffer(ref ibDesc);
        }

        uint vertexOffset = 0;
        uint indexOffset = 0;

        for (int i = 0; i < dataPtr.CmdListsCount; i++)
        {
            ImDrawListPtr listPtr = dataPtr.CmdLists.Data[i];

            uint vertexSize = (uint)(listPtr.VtxBuffer.Size * sizeof(ImDrawVert));
            uint indexSize = (uint)(listPtr.IdxBuffer.Size * sizeof(ushort));

            Context.UpdateBuffer(vertexBuffer,
                                 (nint)listPtr.VtxBuffer.Data,
                                 vertexSize,
                                 vertexOffset);

            Context.UpdateBuffer(indexBuffer,
                                 (nint)listPtr.IdxBuffer.Data,
                                 indexSize,
                                 indexOffset);

            vertexOffset += vertexSize;
            indexOffset += indexSize;
        }

        Vector2 displayPos = dataPtr.DisplayPos;
        Vector2 displaySize = dataPtr.DisplaySize;

        Matrix4X4<float> projection = Matrix4X4.CreateOrthographicOffCenter(displayPos.X,
                                                                            displayPos.X + displaySize.X,
                                                                            displayPos.Y + displaySize.Y,
                                                                            displayPos.Y,
                                                                            -1.0f,
                                                                            1.0f);

        Context.UpdateBuffer(constantsBuffer, (nint)(&projection), (uint)sizeof(Matrix4X4<float>));

        commandBuffer.SetGraphicsPipeline(pipeline);
        commandBuffer.SetVertexBuffer(0, vertexBuffer);
        commandBuffer.SetIndexBuffer(indexBuffer, IndexFormat.U16Bit);
        commandBuffer.SetResourceSet(0, set0);
        commandBuffer.SetResourceSet(1, set1);

        vertexOffset = 0;
        indexOffset = 0;

        for (int i = 0; i < dataPtr.CmdListsCount; i++)
        {
            ImDrawListPtr listPtr = dataPtr.CmdLists.Data[i];

            for (int j = 0; j < listPtr.CmdBuffer.Size; j++)
            {
                ImDrawCmd cmd = listPtr.CmdBuffer.Data[j];

                if (cmd.UserCallback != null)
                {
                    ImDrawCallback callback = Marshal.GetDelegateForFunctionPointer<ImDrawCallback>((nint)cmd.UserCallback);

                    callback(listPtr, &cmd);
                }
                else
                {
                    Rectangle<int> scissor = new((int)Math.Max(0, cmd.ClipRect.X - displayPos.X),
                                                 (int)Math.Max(0, cmd.ClipRect.Y - displayPos.Y),
                                                 (int)Math.Max(0, cmd.ClipRect.Z - cmd.ClipRect.X),
                                                 (int)Math.Max(0, cmd.ClipRect.W - cmd.ClipRect.Y));

                    commandBuffer.SetViewport(0, new Viewport(0, 0, displaySize.X, displaySize.Y, 0.0f, 1.0f));
                    commandBuffer.SetScissorRectangle(0, scissor);

                    commandBuffer.DrawIndexed(cmd.ElemCount,
                                              cmd.IdxOffset + indexOffset,
                                              cmd.VtxOffset + vertexOffset);
                }
            }

            vertexOffset += (uint)listPtr.VtxBuffer.Size;
            indexOffset += (uint)listPtr.IdxBuffer.Size;
        }
    }

    protected override void Destroy()
    {
        set1.Dispose();

        fontTextureView.Dispose();
        fontTexture.Dispose();

        set0.Dispose();

        pipeline.Dispose();
        layout1.Dispose();
        layout0.Dispose();
        sampler.Dispose();
        constantsBuffer.Dispose();
        indexBuffer.Dispose();
        vertexBuffer.Dispose();
    }

    private void CreateGraphicsResources(OutputDesc outputDesc, ColorSpaceHandling handling)
    {
        BufferDesc vbDesc = BufferDesc.Default((uint)sizeof(ImDrawVert) * 5000,
                                               BufferUsage.VertexBuffer | BufferUsage.Dynamic);

        vertexBuffer = Context.Factory.CreateBuffer(in vbDesc);

        BufferDesc ibDesc = BufferDesc.Default(sizeof(ushort) * 10000,
                                               BufferUsage.IndexBuffer | BufferUsage.Dynamic);

        indexBuffer = Context.Factory.CreateBuffer(in ibDesc);

        BufferDesc cbDesc = BufferDesc.Default((uint)sizeof(Matrix4X4<float>),
                                               BufferUsage.ConstantBuffer | BufferUsage.Dynamic);

        constantsBuffer = Context.Factory.CreateBuffer(in cbDesc);

        SamplerDesc samplerDesc = SamplerDesc.Default(filter: SamplerFilter.MinPointMagPointMipPoint);

        sampler = Context.Factory.CreateSampler(in samplerDesc);

        ResourceLayoutDesc layout0Desc = ResourceLayoutDesc.Default(
        [
            LayoutElementDesc.Default(ShaderStages.Vertex, ResourceType.ConstantBuffer, 0),
            LayoutElementDesc.Default(ShaderStages.Pixel, ResourceType.Sampler, 0)
        ]);

        layout0 = Context.Factory.CreateResourceLayout(in layout0Desc);

        ResourceLayoutDesc layout1Desc = ResourceLayoutDesc.Default(
        [
            LayoutElementDesc.Default(ShaderStages.Pixel, ResourceType.Texture, 0)
        ]);

        layout1 = Context.Factory.CreateResourceLayout(in layout1Desc);

        Shaders.Get(handling, out byte[] vs, out byte[] ps);

        ShaderDesc shaderDesc = ShaderDesc.Default(ShaderStages.Vertex, vs, Shaders.VSMain);

        using Shader vsShader = Context.Factory.CreateShader(in shaderDesc);

        shaderDesc = ShaderDesc.Default(ShaderStages.Pixel, ps, Shaders.PSMain);

        using Shader psShader = Context.Factory.CreateShader(in shaderDesc);

        LayoutDesc layoutDesc = LayoutDesc.Default();
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.Position, 0));
        layoutDesc.Add(ElementDesc.Default(ElementFormat.Float2, ElementSemanticType.TexCoord, 0));
        layoutDesc.Add(ElementDesc.Default(ElementFormat.UByte4Normalized, ElementSemanticType.Color, 0));

        RenderStateDesc renderStateDesc = RenderStateDesc.Default();
        renderStateDesc.RasterizerState = RasterizerStateDesc.Default(CullMode.None);
        renderStateDesc.DepthStencilState = DepthStencilStateDesc.Default(false);

        GraphicsPipelineDesc pipelineDesc = GraphicsPipelineDesc.Default
        (
            GraphicsShaderDesc.Default(vertex: vsShader, pixel: psShader),
            [layoutDesc],
            [layout0, layout1],
            outputDesc,
            renderStateDesc
        );

        pipeline = Context.Factory.CreateGraphicsPipeline(in pipelineDesc);

        ResourceSetDesc set0Desc = ResourceSetDesc.Default(layout0, constantsBuffer, sampler);

        set0 = Context.Factory.CreateResourceSet(in set0Desc);

        ImGuiIOPtr io = ImGuiApi.GetIO();

        byte* pixels;
        int width;
        int height;
        io.Fonts.GetTexDataAsRGBA32(&pixels, &width, &height);

        TextureDesc textureDesc = TextureDesc.Default((uint)width, (uint)height, 1, 1);

        fontTexture = Context.Factory.CreateTexture(in textureDesc);

        TextureViewDesc textureViewDesc = TextureViewDesc.Default(fontTexture);

        fontTextureView = Context.Factory.CreateTextureView(in textureViewDesc);

        Context.UpdateTexture(fontTexture,
                              (nint)pixels,
                              (uint)(width * height * 4),
                              new(0, 0, 0, (uint)width, (uint)height, 1, 0));

        ResourceSetDesc set1Desc = ResourceSetDesc.Default(layout1, fontTextureView);

        set1 = Context.Factory.CreateResourceSet(in set1Desc);
    }
}
