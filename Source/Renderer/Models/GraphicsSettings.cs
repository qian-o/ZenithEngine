using CommunityToolkit.Mvvm.ComponentModel;
using Graphics.Core;
using Renderer.Components;

namespace Renderer.Models;

internal sealed partial class GraphicsSettings : MVVM
{
    [ObservableProperty]
    private TextureSampleCount sampleCount = TextureSampleCount.Count1;

    protected override void Destroy()
    {
    }
}
