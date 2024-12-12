using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Graphics;

public static class SamplerStates
{
    /// <summary>
    /// SamplerState description using point filter (bilinear) and clamp address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc PointClamp;

    /// <summary>
    /// SamplerState description using a point filter (bilinear) and a wrap address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc PointWrap;

    /// <summary>
    /// SamplerState description using point filter (bilinear) and mirror address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc PointMirror;

    /// <summary>
    /// SamplerState description using a linear filter (trilinear) and clamp address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc LinearClamp;

    /// <summary>
    /// SamplerState description using a linear filter (trilinear) and wrap address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc LinearWrap;

    /// <summary>
    /// SamplerState description using a linear filter (trilinear) and a mirror address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc LinearMirror;

    /// <summary>
    /// SamplerState description using an anisotropic filter and clamp address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc AnisotropicClamp;

    /// <summary>
    /// SamplerState description using an anisotropic filter and wrap address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc AnisotropicWrap;

    /// <summary>
    /// Describes the SamplerState using anisotropic filtering and mirror address mode for UVW.
    /// </summary>
    public static readonly SamplerDesc AnisotropicMirror;

    static SamplerStates()
    {
        PointClamp = SamplerDesc.Default();
        PointClamp.Filter = SamplerFilter.MinPointMagPointMipPoint;

        PointWrap = SamplerDesc.Default();
        PointClamp.Filter = SamplerFilter.MinPointMagPointMipPoint;
        PointWrap.AddressModeU = AddressMode.Wrap;
        PointWrap.AddressModeV = AddressMode.Wrap;
        PointWrap.AddressModeW = AddressMode.Wrap;

        PointMirror = SamplerDesc.Default();
        PointClamp.Filter = SamplerFilter.MinPointMagPointMipPoint;
        PointMirror.AddressModeU = AddressMode.Mirror;
        PointMirror.AddressModeV = AddressMode.Mirror;
        PointMirror.AddressModeW = AddressMode.Mirror;

        LinearClamp = SamplerDesc.Default();

        LinearWrap = SamplerDesc.Default();
        LinearWrap.AddressModeU = AddressMode.Wrap;
        LinearWrap.AddressModeV = AddressMode.Wrap;
        LinearWrap.AddressModeW = AddressMode.Wrap;

        LinearMirror = SamplerDesc.Default();
        LinearMirror.AddressModeU = AddressMode.Mirror;
        LinearMirror.AddressModeV = AddressMode.Mirror;
        LinearMirror.AddressModeW = AddressMode.Mirror;

        AnisotropicClamp = SamplerDesc.Default();
        AnisotropicClamp.Filter = SamplerFilter.Anisotropic;

        AnisotropicWrap = SamplerDesc.Default();
        AnisotropicWrap.Filter = SamplerFilter.Anisotropic;
        AnisotropicWrap.AddressModeU = AddressMode.Wrap;
        AnisotropicWrap.AddressModeV = AddressMode.Wrap;
        AnisotropicWrap.AddressModeW = AddressMode.Wrap;

        AnisotropicMirror = SamplerDesc.Default();
        AnisotropicMirror.Filter = SamplerFilter.Anisotropic;
        AnisotropicMirror.AddressModeU = AddressMode.Mirror;
        AnisotropicMirror.AddressModeV = AddressMode.Mirror;
        AnisotropicMirror.AddressModeW = AddressMode.Mirror;
    }
}
