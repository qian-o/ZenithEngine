using CommunityToolkit.Mvvm.ComponentModel;
using Graphics.Core;
using Renderer.Components;

namespace Renderer.Models;

internal sealed partial class Settings : MVVM
{
    [ObservableProperty]
    private TextureSampleCount sampleCount = TextureSampleCount.Count1;

    [ObservableProperty]
    private bool isMultiThreadedRendering = false;

    protected override void Destroy()
    {
    }
}
