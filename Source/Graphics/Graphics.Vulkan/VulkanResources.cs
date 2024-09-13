using Graphics.Core;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Graphics.Vulkan;

public class VulkanResources : DisposableObject
{
    #region Global Properties
    public Alloter Alloter { get; } = new();
    #endregion

    #region Context Properties
    private Vk? _vk;
    private VkInstance? _instance;
    private ExtDebugUtils? _extDebugUtils;
    private KhrSurface? _khrSurface;

    public bool IsInitializedContext { get; private set; }

    public Vk Vk => TryGetContextProperty(ref _vk)!;

    public VkInstance Instance => TryGetContextProperty(ref _instance)!.Value;

    public ExtDebugUtils? ExtDebugUtils => TryGetContextProperty(ref _extDebugUtils);

    public KhrSurface KhrSurface => TryGetContextProperty(ref _khrSurface)!;
    #endregion

    #region Physical Device Properties
    private PhysicalDevice? _physicalDevice;
    private PhysicalDeviceProperties? _properties;
    private PhysicalDeviceDescriptorBufferPropertiesEXT? _descriptorBufferProperties;
    private PhysicalDeviceDescriptorIndexingProperties? _descriptorIndexingProperties;
    private PhysicalDeviceFeatures? _features;
    private PhysicalDeviceMemoryProperties? _memoryProperties;
    private QueueFamilyProperties[]? _queueFamilyProperties;
    private ExtensionProperties[]? _extensionProperties;

    public bool IsInitializedPhysicalDevice { get; private set; }

    public PhysicalDevice PhysicalDevice => TryGetPhysicalDeviceProperty(ref _physicalDevice)!;

    public PhysicalDeviceProperties Properties => TryGetPhysicalDeviceProperty(ref _properties)!.Value;

    public PhysicalDeviceDescriptorBufferPropertiesEXT DescriptorBufferProperties => TryGetPhysicalDeviceProperty(ref _descriptorBufferProperties)!.Value;

    public PhysicalDeviceDescriptorIndexingProperties DescriptorIndexingProperties => TryGetPhysicalDeviceProperty(ref _descriptorIndexingProperties)!.Value;

    public PhysicalDeviceFeatures Features => TryGetPhysicalDeviceProperty(ref _features)!.Value;

    public PhysicalDeviceMemoryProperties MemoryProperties => TryGetPhysicalDeviceProperty(ref _memoryProperties)!.Value;

    public QueueFamilyProperties[] QueueFamilyProperties => TryGetPhysicalDeviceProperty(ref _queueFamilyProperties)!;

    public ExtensionProperties[] ExtensionProperties => TryGetPhysicalDeviceProperty(ref _extensionProperties)!;
    #endregion

    #region Graphics Device Properties
    private GraphicsDevice? _graphicsDevice;
    private KhrSwapchain? _khrSwapchain;
    private ExtDescriptorBuffer? _extDescriptorBuffer;
    private Queue? _graphicsQueue;
    private Queue? _computeQueue;
    private Queue? _transferQueue;
    private CommandPool? _graphicsCommandPool;
    private CommandPool? _computeCommandPool;

    public bool IsInitializedGraphicsDevice { get; private set; }

    public GraphicsDevice GraphicsDevice => TryGetGraphicsDeviceProperty(ref _graphicsDevice)!;

    public KhrSwapchain KhrSwapchain => TryGetGraphicsDeviceProperty(ref _khrSwapchain)!;

    public ExtDescriptorBuffer ExtDescriptorBuffer => TryGetGraphicsDeviceProperty(ref _extDescriptorBuffer)!;

    public Queue GraphicsQueue => TryGetGraphicsDeviceProperty(ref _graphicsQueue)!;

    public Queue ComputeQueue => TryGetGraphicsDeviceProperty(ref _computeQueue)!;

    public Queue TransferQueue => TryGetGraphicsDeviceProperty(ref _transferQueue)!;

    public CommandPool GraphicsCommandPool => TryGetGraphicsDeviceProperty(ref _graphicsCommandPool)!;

    public CommandPool ComputeCommandPool => TryGetGraphicsDeviceProperty(ref _computeCommandPool)!;
    #endregion

    public void InitializeContext(Vk vk, VkInstance instance, ExtDebugUtils? extDebugUtils, KhrSurface khrSurface)
    {
        _vk = vk;
        _instance = instance;
        _extDebugUtils = extDebugUtils;
        _khrSurface = khrSurface;

        IsInitializedContext = true;
    }

    public void InitializePhysicalDevice(PhysicalDevice physicalDevice,
                                         PhysicalDeviceProperties properties,
                                         PhysicalDeviceDescriptorBufferPropertiesEXT descriptorBufferProperties,
                                         PhysicalDeviceDescriptorIndexingProperties descriptorIndexingProperties,
                                         PhysicalDeviceFeatures features,
                                         PhysicalDeviceMemoryProperties memoryProperties,
                                         QueueFamilyProperties[] queueFamilyProperties,
                                         ExtensionProperties[] extensionProperties)
    {
        _physicalDevice = physicalDevice;
        _properties = properties;
        _descriptorBufferProperties = descriptorBufferProperties;
        _descriptorIndexingProperties = descriptorIndexingProperties;
        _features = features;
        _memoryProperties = memoryProperties;
        _queueFamilyProperties = queueFamilyProperties;
        _extensionProperties = extensionProperties;

        IsInitializedPhysicalDevice = true;
    }

    public void InitializeGraphicsDevice(GraphicsDevice graphicsDevice,
                                         KhrSwapchain khrSwapchain,
                                         ExtDescriptorBuffer extDescriptorBuffer,
                                         Queue graphicsQueue,
                                         Queue computeQueue,
                                         Queue transferQueue,
                                         CommandPool graphicsCommandPool,
                                         CommandPool computeCommandPool)
    {
        _graphicsDevice = graphicsDevice;
        _khrSwapchain = khrSwapchain;
        _extDescriptorBuffer = extDescriptorBuffer;
        _graphicsQueue = graphicsQueue;
        _computeQueue = computeQueue;
        _transferQueue = transferQueue;
        _graphicsCommandPool = graphicsCommandPool;
        _computeCommandPool = computeCommandPool;

        IsInitializedGraphicsDevice = true;
    }

    protected override void Destroy()
    {
        Alloter.Dispose();
    }

    private T TryGetContextProperty<T>(ref T field)
    {
        if (!IsInitializedContext)
        {
            throw new InvalidOperationException("Vulkan context is not initialized.");
        }

        return field;
    }

    private T TryGetPhysicalDeviceProperty<T>(ref T field)
    {
        if (!IsInitializedPhysicalDevice)
        {
            throw new InvalidOperationException("Vulkan physical device is not initialized.");
        }

        return field;
    }

    private T TryGetGraphicsDeviceProperty<T>(ref T field)
    {
        if (!IsInitializedGraphicsDevice)
        {
            throw new InvalidOperationException("Graphics device is not initialized.");
        }

        return field;
    }
}
