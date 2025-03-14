﻿using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public static class Samplers
{
    /// <summary>
    /// Sampler description using point filter (bilinear) and clamp address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc PointClamp;

    /// <summary>
    /// Sampler description using a point filter (bilinear) and a wrap address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc PointWrap;

    /// <summary>
    /// Sampler description using point filter (bilinear) and mirror address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc PointMirror;

    /// <summary>
    /// Sampler description using a linear filter (trilinear) and clamp address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc LinearClamp;

    /// <summary>
    /// Sampler description using a linear filter (trilinear) and wrap address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc LinearWrap;

    /// <summary>
    /// Samplere description using a linear filter (trilinear) and a mirror address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc LinearMirror;

    /// <summary>
    /// Sampler description using an anisotropic filter and clamp address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc AnisotropicClamp;

    /// <summary>
    /// Sampler description using an anisotropic filter and wrap address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc AnisotropicWrap;

    /// <summary>
    /// Describes the Sampler using anisotropic filtering and mirror address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc AnisotropicMirror;

    static Samplers()
    {
        PointClamp = new
        (
            addressModeU: AddressMode.Clamp,
            addressModeV: AddressMode.Clamp,
            addressModeW: AddressMode.Clamp,
            filter: SamplerFilter.MinPointMagPointMipPoint
        );

        PointWrap = new(filter: SamplerFilter.MinPointMagPointMipPoint);

        PointMirror = new
        (
            addressModeU: AddressMode.Mirror,
            addressModeV: AddressMode.Mirror,
            addressModeW: AddressMode.Mirror,
            filter: SamplerFilter.MinPointMagPointMipPoint
        );

        LinearClamp = new
        (
            addressModeU: AddressMode.Clamp,
            addressModeV: AddressMode.Clamp,
            addressModeW: AddressMode.Clamp
        );

        LinearWrap = new();

        LinearMirror = new
        (
            addressModeU: AddressMode.Mirror,
            addressModeV: AddressMode.Mirror,
            addressModeW: AddressMode.Mirror
        );

        AnisotropicClamp = new
        (
            addressModeU: AddressMode.Clamp,
            addressModeV: AddressMode.Clamp,
            addressModeW: AddressMode.Clamp,
            filter: SamplerFilter.Anisotropic
        );

        AnisotropicWrap = new(filter: SamplerFilter.Anisotropic);

        AnisotropicMirror = new
        (
            addressModeU: AddressMode.Mirror,
            addressModeV: AddressMode.Mirror,
            addressModeW: AddressMode.Mirror,
            filter: SamplerFilter.Anisotropic
        );
    }
}
