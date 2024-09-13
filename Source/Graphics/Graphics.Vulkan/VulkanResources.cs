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

    public bool IsInitializedGraphicsDevice { get; private set; }

    public GraphicsDevice GraphicsDevice => TryGetGraphicsDeviceProperty(ref _graphicsDevice)!;
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

    public void InitializeGraphicsDevice(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

        IsInitializedGraphicsDevice = true;
    }

    public VkPhysicalDevice GetPhysicalDevice()
    {
        return PhysicalDevice.Handle;
    }

    public VkDevice GetDevice()
    {
        return GraphicsDevice.Handle;
    }

    public KhrSwapchain GetKhrSwapchain()
    {
        return GraphicsDevice.KhrSwapchain;
    }

    public ExtDescriptorBuffer GetExtDescriptorBuffer()
    {
        return GraphicsDevice.ExtDescriptorBuffer;
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
