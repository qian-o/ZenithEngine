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

            ImGui.EndMainMenuBar();
        }
    }
}
