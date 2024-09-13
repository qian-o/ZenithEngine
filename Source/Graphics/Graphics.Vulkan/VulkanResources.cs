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

    public bool IsInitializedPhysicalDevice { get; private set; }

    public PhysicalDevice PhysicalDevice => TryGetPhysicalDeviceProperty(ref _physicalDevice)!;

    public VkPhysicalDevice VkPhysicalDevice => PhysicalDevice.Handle;

    public PhysicalDeviceProperties Properties => PhysicalDevice.Properties;

    public PhysicalDeviceDescriptorBufferPropertiesEXT DescriptorBufferProperties => PhysicalDevice.DescriptorBufferProperties;

    public PhysicalDeviceDescriptorIndexingProperties DescriptorIndexingProperties => PhysicalDevice.DescriptorIndexingProperties;

    public PhysicalDeviceFeatures Features => PhysicalDevice.Features;

    public PhysicalDeviceMemoryProperties MemoryProperties => PhysicalDevice.MemoryProperties;

    public QueueFamilyProperties[] QueueFamilyProperties => PhysicalDevice.QueueFamilyProperties;

    public ExtensionProperties[] ExtensionProperties => PhysicalDevice.ExtensionProperties;
    #endregion

    #region Graphics Device Properties
    private GraphicsDevice? _graphicsDevice;

    public bool IsInitializedGraphicsDevice { get; private set; }

    public GraphicsDevice GraphicsDevice => TryGetGraphicsDeviceProperty(ref _graphicsDevice)!;

    public VkDevice VkDevice => GraphicsDevice.Handle;

    public KhrSwapchain KhrSwapchain => GraphicsDevice.KhrSwapchain;

    public ExtDescriptorBuffer ExtDescriptorBuffer => GraphicsDevice.ExtDescriptorBuffer;
    #endregion

    public void InitializeContext(Vk vk, VkInstance instance, ExtDebugUtils? extDebugUtils, KhrSurface khrSurface)
    {
        _vk = vk;
        _instance = instance;
        _extDebugUtils = extDebugUtils;
        _khrSurface = khrSurface;

        IsInitializedContext = true;
    }

    public void InitializePhysicalDevice(PhysicalDevice physicalDevice)
    {
        _physicalDevice = physicalDevice;

        IsInitializedPhysicalDevice = true;
    }

    public void InitializeGraphicsDevice(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;

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
