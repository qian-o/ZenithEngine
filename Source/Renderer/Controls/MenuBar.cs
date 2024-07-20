using Graphics.Core;
using Hexa.NET.ImGui;
using Renderer.Components;

namespace Renderer.Controls;

internal sealed class MenuBar(MainWindow mainWindow) : Control(mainWindow)
{
    protected override void Initialize()
    {
    }

    protected override void UpdateCore(UpdateEventArgs e)
    {
    }

    protected override void RenderCore(RenderEventArgs e)
    {
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("File"))
            {
                if (ImGui.MenuItem("New"))
                {
                }

                if (ImGui.MenuItem("Open"))
                {
                }

                if (ImGui.MenuItem("Save"))
                {
                }

                if (ImGui.MenuItem("Save As"))
                {
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Exit"))
                {
                    App.Exit();
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Settings"))
            {
                if (ImGui.BeginMenu("MSAA"))
                {
                    if (ImGui.MenuItem("Disabled", App.GraphicsSettings.SampleCount == TextureSampleCount.Count1))
                    {
                        App.GraphicsSettings.SampleCount = TextureSampleCount.Count1;
                    }

                    if (ImGui.MenuItem("2x", App.GraphicsSettings.SampleCount == TextureSampleCount.Count2))
                    {
                        App.GraphicsSettings.SampleCount = TextureSampleCount.Count2;
                    }

                    if (ImGui.MenuItem("4x", App.GraphicsSettings.SampleCount == TextureSampleCount.Count4))
                    {
                        App.GraphicsSettings.SampleCount = TextureSampleCount.Count4;
                    }

                    if (ImGui.MenuItem("8x", App.GraphicsSettings.SampleCount == TextureSampleCount.Count8))
                    {
                        App.GraphicsSettings.SampleCount = TextureSampleCount.Count8;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMainMenuBar();
        }
    }
}
