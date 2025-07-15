using Hexa.NET.ImGui;

namespace ZenithEngine.Editor.Views;

public class MainView : IView
{
    public void Render()
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

                if (ImGui.MenuItem("Exit"))
                {
                    App.Exit();
                }

                ImGui.EndMenu();
            }
        }
        ImGui.EndMainMenuBar();
    }
}
