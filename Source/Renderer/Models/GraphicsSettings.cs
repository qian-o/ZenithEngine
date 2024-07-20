using CommunityToolkit.Mvvm.ComponentModel;
using Graphics.Core;

namespace Renderer.Models;

internal sealed partial class GraphicsSettings : MVVM
{
    [ObservableProperty]
    private TextureSampleCount sampleCount = TextureSampleCount.Count1;

    protected override void Destroy()
    {
    }
}
