using Metal;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Metal;

internal static class MTLResourceHelper
{
    /// <summary>
    /// Determines if hazard tracking should be disabled for a resource with the given usage flags.
    /// Hazard tracking mode can be disabled for resources that manage their own synchronization,
    /// typically compute UAV resources that don't require automatic dependency tracking.
    /// </summary>
    /// <param name="hasUnorderedAccess">Whether the resource has unordered access usage.</param>
    /// <param name="isDynamic">Whether the resource is dynamic (CPU accessible).</param>
    /// <param name="isRenderTarget">Whether the resource is a render target.</param>
    /// <returns>True if hazard tracking should be disabled, false otherwise.</returns>
    public static bool ShouldDisableHazardTracking(bool hasUnorderedAccess, bool isDynamic, bool isRenderTarget)
    {
        // Disable hazard tracking for UAV resources that:
        // 1. Have unordered access usage
        // 2. Are not dynamic (not CPU accessible)
        // 3. Are not render targets (which need automatic synchronization)
        return hasUnorderedAccess && !isDynamic && !isRenderTarget;
    }

    /// <summary>
    /// Gets the hazard tracking mode option based on whether it should be disabled.
    /// </summary>
    /// <param name="shouldDisable">Whether hazard tracking should be disabled.</param>
    /// <returns>The appropriate MTLResourceOptions flag, or none if tracking should remain enabled.</returns>
    public static MTLResourceOptions GetHazardTrackingMode(bool shouldDisable)
    {
        return shouldDisable ? MTLResourceOptions.HazardTrackingModeUntracked : MTLResourceOptions.HazardTrackingModeTracked;
    }
}
