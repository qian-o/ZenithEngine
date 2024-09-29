using System.Numerics;
using System.Text;
using Graphics.Core;
using Silk.NET.Vulkan;

namespace Graphics.Vulkan;

internal sealed unsafe class Program
{
    private struct Vertex(Vector3 position, Vector3 normal, Vector3 color, Vector2 texCoord)
    {
        public Vector3 Position = position;

        public Vector3 Normal = normal;

        public Vector3 Color = color;

        public Vector2 TexCoord = texCoord;
    }

    private sealed class AccelerationStructure(DeviceBuffer buffer)
    {
        public DeviceBuffer Buffer { get; } = buffer;

        public AccelerationStructureKHR Handle { get; set; }

        public ulong Address { get; set; }
    }

    private static GraphicsDevice _device = null!;
    private static ImGuiController _imGuiController = null!;
    private static CommandList _commandList = null!;

    private static DeviceBuffer vertexBuffer = null!;
    private static DeviceBuffer indexBuffer = null!;
    private static DeviceBuffer transformBuffer = null!;
    private static AccelerationStructure bottomLevelAS = null!;
    private static AccelerationStructure topLevelAS = null!;
    private static DescriptorSetLayout descriptorSetLayout;
    private static PipelineLayout pipelineLayout;

    private static void Main(string[] _)
    {
        using Window window = Window.CreateWindowByVulkan();
        window.Title = "Graphics.Vulkan";
        window.MinimumSize = new(100, 100);

        using Context context = new();
        using GraphicsDevice device = context.CreateGraphicsDevice(context.GetBestPhysicalDevice(), window);
        using ImGuiController imGuiController = new(window, device);
        using CommandList commandList = device.Factory.CreateGraphicsCommandList();

        window.Load += Load;
        window.Update += Update;
        window.Render += Render;
        window.Resize += Resize;

        _device = device;
        _imGuiController = imGuiController;
        _commandList = commandList;

        window.Run();
    }

    private static void Load(object? sender, LoadEventArgs e)
    {
        CreateBottomLevelAccelerationStructure();
        CreateTopLevelAccelerationStructure();
        CreateRayTracingPipeline();
    }

    private static void Update(object? sender, UpdateEventArgs e)
    {
    }

    private static void Render(object? sender, RenderEventArgs e)
    {
    }

    private static void Resize(object? sender, ResizeEventArgs e)
    {
        _device.MainSwapchain.Resize(e.Width, e.Height);
    }

    private static void CreateBottomLevelAccelerationStructure()
    {
        Vertex[] vertices =
        [
            new(new(0.0f, 0.5f, 0.0f), new(0.0f, 0.0f, -1.0f), new(1.0f, 0.0f, 0.0f), new(0.0f, 0.0f)),
            new(new(0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, -1.0f), new(0.0f, 1.0f, 0.0f), new(0.0f, 1.0f)),
            new(new(-0.5f, -0.5f, 0.0f), new(0.0f, 0.0f, -1.0f), new(0.0f, 0.0f, 1.0f), new(1.0f, 1.0f))
        ];

        uint[] indices = [0, 1, 2];

        TransformMatrixKHR transformMatrix = new();
        transformMatrix.Matrix[0] = 1.0f;
        transformMatrix.Matrix[5] = 1.0f;
        transformMatrix.Matrix[10] = 1.0f;

        vertexBuffer = new DeviceBuffer(_device.VkRes,
                                        BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                        (uint)(vertices.Length * sizeof(Vertex)),
                                        true);
        _device.UpdateBuffer(vertexBuffer, 0, vertices);

        indexBuffer = new DeviceBuffer(_device.VkRes,
                                       BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                       (uint)(indices.Length * sizeof(uint)),
                                       true);
        _device.UpdateBuffer(indexBuffer, 0, indices);

        transformBuffer = new DeviceBuffer(_device.VkRes,
                                           BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                           (uint)sizeof(TransformMatrixKHR),
                                           true);
        _device.UpdateBuffer(transformBuffer, 0, in transformMatrix);

        AccelerationStructureGeometryKHR accelerationStructureGeometry = new()
        {
            SType = StructureType.AccelerationStructureGeometryKhr,
            GeometryType = GeometryTypeKHR.TrianglesKhr,
            Geometry = new()
            {
                Triangles = new()
                {
                    SType = StructureType.AccelerationStructureGeometryTrianglesDataKhr,
                    VertexFormat = Format.R32G32B32Sfloat,
                    VertexData = new DeviceOrHostAddressConstKHR
                    {
                        DeviceAddress = vertexBuffer.Address
                    },
                    VertexStride = (uint)sizeof(Vertex),
                    MaxVertex = (uint)vertices.Length,
                    IndexType = IndexType.Uint32,
                    IndexData = new DeviceOrHostAddressConstKHR
                    {
                        DeviceAddress = indexBuffer.Address
                    },
                    TransformData = new DeviceOrHostAddressConstKHR
                    {
                        DeviceAddress = transformBuffer.Address
                    }
                }
            },
            Flags = GeometryFlagsKHR.OpaqueBitKhr
        };

        AccelerationStructureBuildGeometryInfoKHR accelerationStructureBuildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr,
            GeometryCount = 1,
            PGeometries = &accelerationStructureGeometry,
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };

        uint numTriangles = 1;
        AccelerationStructureBuildSizesInfoKHR accelerationStructureBuildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };
        _device.VkRes.KhrAccelerationStructure.GetAccelerationStructureBuildSizes(_device.VkRes.VkDevice,
                                                                                  AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                                  &accelerationStructureBuildGeometryInfo,
                                                                                  &numTriangles,
                                                                                  &accelerationStructureBuildSizesInfo);

        bottomLevelAS = new AccelerationStructure(new DeviceBuffer(_device.VkRes,
                                                                   BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureStorageBitKhr,
                                                                   (uint)accelerationStructureBuildSizesInfo.AccelerationStructureSize,
                                                                   false));

        AccelerationStructureCreateInfoKHR accelerationStructureCreateInfo = new()
        {
            SType = StructureType.AccelerationStructureCreateInfoKhr,
            Buffer = bottomLevelAS.Buffer.Handle,
            Size = accelerationStructureBuildSizesInfo.AccelerationStructureSize,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr
        };

        AccelerationStructureKHR accelerationStructure;
        _device.VkRes.KhrAccelerationStructure.CreateAccelerationStructure(_device.VkRes.VkDevice, &accelerationStructureCreateInfo, null, &accelerationStructure).ThrowCode();
        bottomLevelAS.Handle = accelerationStructure;

        DeviceBuffer scratchBuffer = new(_device.VkRes,
                                         BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.StorageBufferBit,
                                         (uint)accelerationStructureBuildSizesInfo.BuildScratchSize,
                                         false);

        AccelerationStructureBuildGeometryInfoKHR accelerationBuildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.BottomLevelKhr,
            DstAccelerationStructure = bottomLevelAS.Handle,
            GeometryCount = 1,
            PGeometries = &accelerationStructureGeometry,
            ScratchData = new DeviceOrHostAddressKHR
            {
                DeviceAddress = scratchBuffer.Address
            },
            Mode = BuildAccelerationStructureModeKHR.BuildKhr,
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };

        AccelerationStructureBuildRangeInfoKHR accelerationStructureBuildRangeInfo = new()
        {
            PrimitiveCount = numTriangles,
            PrimitiveOffset = 0,
            FirstVertex = 0,
            TransformOffset = 0
        };

        AccelerationStructureBuildRangeInfoKHR[] accelerationStructureBuildRangeInfos = [accelerationStructureBuildRangeInfo];
        AccelerationStructureBuildRangeInfoKHR* accelerationStructureBuildRangeInfosPtr = accelerationStructureBuildRangeInfos.AsPointer();

        CommandList commandList = _device.Factory.CreateGraphicsCommandList();

        commandList.Begin();

        _device.VkRes.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandList.Handle,
                                                                             1,
                                                                             &accelerationBuildGeometryInfo,
                                                                             &accelerationStructureBuildRangeInfosPtr);

        commandList.End();

        _device.SubmitCommands(commandList);

        AccelerationStructureDeviceAddressInfoKHR accelerationDeviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = bottomLevelAS.Handle
        };

        bottomLevelAS.Address = _device.VkRes.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(_device.VkRes.VkDevice,
                                                                                                             &accelerationDeviceAddressInfo);

        scratchBuffer.Dispose();
    }

    private static void CreateTopLevelAccelerationStructure()
    {
        TransformMatrixKHR transformMatrix = new();
        transformMatrix.Matrix[0] = 1.0f;
        transformMatrix.Matrix[5] = 1.0f;
        transformMatrix.Matrix[10] = 1.0f;

        AccelerationStructureInstanceKHR instance = new()
        {
            Transform = transformMatrix,
            InstanceCustomIndex = 0,
            Mask = 0xFF,
            InstanceShaderBindingTableRecordOffset = 0,
            AccelerationStructureReference = bottomLevelAS.Address,
            Flags = GeometryInstanceFlagsKHR.TriangleFacingCullDisableBitKhr
        };

        DeviceBuffer instanceBuffer = new(_device.VkRes,
                                          BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureBuildInputReadOnlyBitKhr,
                                          (uint)sizeof(AccelerationStructureInstanceKHR),
                                          true);
        _device.UpdateBuffer(instanceBuffer, 0, in instance);

        DeviceOrHostAddressConstKHR instanceData = new()
        {
            DeviceAddress = instanceBuffer.Address
        };

        AccelerationStructureGeometryKHR accelerationStructureGeometry = new()
        {
            SType = StructureType.AccelerationStructureGeometryKhr,
            GeometryType = GeometryTypeKHR.InstancesKhr,
            Geometry = new()
            {
                Instances = new()
                {
                    SType = StructureType.AccelerationStructureGeometryInstancesDataKhr,
                    Data = instanceData,
                    ArrayOfPointers = false
                }
            },
            Flags = GeometryFlagsKHR.OpaqueBitKhr
        };

        AccelerationStructureBuildGeometryInfoKHR accelerationStructureBuildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            GeometryCount = 1,
            PGeometries = &accelerationStructureGeometry,
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };

        uint numInstances = 1;

        AccelerationStructureBuildSizesInfoKHR accelerationStructureBuildSizesInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildSizesInfoKhr
        };

        _device.VkRes.KhrAccelerationStructure.GetAccelerationStructureBuildSizes(_device.VkRes.VkDevice,
                                                                                  AccelerationStructureBuildTypeKHR.DeviceKhr,
                                                                                  &accelerationStructureBuildGeometryInfo,
                                                                                  &numInstances,
                                                                                  &accelerationStructureBuildSizesInfo);

        topLevelAS = new AccelerationStructure(new DeviceBuffer(_device.VkRes,
                                                                BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.AccelerationStructureStorageBitKhr,
                                                                (uint)accelerationStructureBuildSizesInfo.AccelerationStructureSize,
                                                                false));

        AccelerationStructureCreateInfoKHR accelerationStructureCreateInfo = new()
        {
            SType = StructureType.AccelerationStructureCreateInfoKhr,
            Buffer = topLevelAS.Buffer.Handle,
            Size = accelerationStructureBuildSizesInfo.AccelerationStructureSize,
            Type = AccelerationStructureTypeKHR.TopLevelKhr
        };

        AccelerationStructureKHR accelerationStructure;
        _device.VkRes.KhrAccelerationStructure.CreateAccelerationStructure(_device.VkRes.VkDevice, &accelerationStructureCreateInfo, null, &accelerationStructure).ThrowCode();
        topLevelAS.Handle = accelerationStructure;

        DeviceBuffer scratchBuffer = new(_device.VkRes,
                                         BufferUsageFlags.ShaderDeviceAddressBit | BufferUsageFlags.StorageBufferBit,
                                         (uint)accelerationStructureBuildSizesInfo.BuildScratchSize,
                                         false);

        AccelerationStructureBuildGeometryInfoKHR accelerationBuildGeometryInfo = new()
        {
            SType = StructureType.AccelerationStructureBuildGeometryInfoKhr,
            Type = AccelerationStructureTypeKHR.TopLevelKhr,
            DstAccelerationStructure = topLevelAS.Handle,
            GeometryCount = 1,
            PGeometries = &accelerationStructureGeometry,
            ScratchData = new DeviceOrHostAddressKHR
            {
                DeviceAddress = scratchBuffer.Address
            },
            Mode = BuildAccelerationStructureModeKHR.BuildKhr,
            Flags = BuildAccelerationStructureFlagsKHR.PreferFastTraceBitKhr
        };

        AccelerationStructureBuildRangeInfoKHR accelerationStructureBuildRangeInfo = new()
        {
            PrimitiveCount = numInstances,
            PrimitiveOffset = 0,
            FirstVertex = 0,
            TransformOffset = 0
        };

        AccelerationStructureBuildRangeInfoKHR[] accelerationStructureBuildRangeInfos = [accelerationStructureBuildRangeInfo];
        AccelerationStructureBuildRangeInfoKHR* accelerationStructureBuildRangeInfosPtr = accelerationStructureBuildRangeInfos.AsPointer();

        CommandList commandList = _device.Factory.CreateGraphicsCommandList();

        commandList.Begin();

        _device.VkRes.KhrAccelerationStructure.CmdBuildAccelerationStructures(commandList.Handle,
                                                                             1,
                                                                             &accelerationBuildGeometryInfo,
                                                                             &accelerationStructureBuildRangeInfosPtr);

        commandList.End();

        _device.SubmitCommands(commandList);

        AccelerationStructureDeviceAddressInfoKHR accelerationDeviceAddressInfo = new()
        {
            SType = StructureType.AccelerationStructureDeviceAddressInfoKhr,
            AccelerationStructure = topLevelAS.Handle
        };

        topLevelAS.Address = _device.VkRes.KhrAccelerationStructure.GetAccelerationStructureDeviceAddress(_device.VkRes.VkDevice,
                                                                                                          &accelerationDeviceAddressInfo);

        scratchBuffer.Dispose();
    }

    private static void CreateRayTracingPipeline()
    {
        DescriptorSetLayoutBinding accelerationStructureLayoutBinding = new()
        {
            Binding = 0,
            DescriptorType = DescriptorType.AccelerationStructureKhr,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.RaygenBitKhr
        };

        DescriptorSetLayoutBinding resultImageLayoutBinding = new()
        {
            Binding = 1,
            DescriptorType = DescriptorType.StorageImage,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.RaygenBitKhr
        };

        DescriptorSetLayoutBinding uniformBufferBinding = new()
        {
            Binding = 2,
            DescriptorType = DescriptorType.UniformBuffer,
            DescriptorCount = 1,
            StageFlags = ShaderStageFlags.RaygenBitKhr
        };

        DescriptorSetLayoutBinding[] bindings =
        [
            accelerationStructureLayoutBinding,
            resultImageLayoutBinding,
            uniformBufferBinding
        ];

        DescriptorSetLayoutCreateInfo descriptorSetLayoutCreateInfo = new()
        {
            SType = StructureType.DescriptorSetLayoutCreateInfo,
            BindingCount = (uint)bindings.Length,
            PBindings = bindings.AsPointer()
        };

        _device.VkRes.Vk.CreateDescriptorSetLayout(_device.VkRes.VkDevice, &descriptorSetLayoutCreateInfo, null, out descriptorSetLayout).ThrowCode();

        PipelineLayoutCreateInfo pipelineLayoutCreateInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 1,
            PSetLayouts = descriptorSetLayout.AsPointer()
        };

        _device.VkRes.Vk.CreatePipelineLayout(_device.VkRes.VkDevice, &pipelineLayoutCreateInfo, null, out pipelineLayout).ThrowCode();

        ShaderDescription rayGenShaderDescription = new()
        {
            ShaderBytes = Encoding.UTF8.GetBytes(File.ReadAllText("Assets/Shaders/raygen.hlsl")),
            Stage = ShaderStages.RayGeneration,
            EntryPoint = "main"
        };

        Shader rayGenShader = _device.Factory.CompileHlslToSpirv(rayGenShaderDescription).First();
    }
}
