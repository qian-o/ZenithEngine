using Common;
using System.Text;
using Hexa.NET.ImGui;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Events;

namespace GamepadTest;

internal unsafe class GamepadInputTest() : VisualTest("Gamepad Input Test")
{
    private readonly Dictionary<GamepadButton, bool> buttonStates = new();
    private readonly Dictionary<GamepadAxis, float> axisValues = new();
    private readonly StringBuilder logBuilder = new();
    private readonly List<string> eventLog = new();
    private int connectedGamepadId = -1;

    protected override void OnLoad()
    {
        // Subscribe to gamepad events
        Window.GamepadButtonDown += OnGamepadButtonDown;
        Window.GamepadButtonUp += OnGamepadButtonUp;
        Window.GamepadAxisMotion += OnGamepadAxisMotion;

        // Initialize button states
        foreach (GamepadButton button in Enum.GetValues<GamepadButton>())
        {
            buttonStates[button] = false;
        }

        // Initialize axis values
        foreach (GamepadAxis axis in Enum.GetValues<GamepadAxis>())
        {
            axisValues[axis] = 0.0f;
        }

        AddToLog("Gamepad test started. Connect a gamepad and press buttons or move sticks.");
    }

    private void OnGamepadButtonDown(object? sender, GamepadButtonEventArgs e)
    {
        buttonStates[e.Button] = true;
        connectedGamepadId = e.GamepadId;
        AddToLog($"Button Down: {e.Button} (Gamepad {e.GamepadId})");
    }

    private void OnGamepadButtonUp(object? sender, GamepadButtonEventArgs e)
    {
        buttonStates[e.Button] = false;
        connectedGamepadId = e.GamepadId;
        AddToLog($"Button Up: {e.Button} (Gamepad {e.GamepadId})");
    }

    private void OnGamepadAxisMotion(object? sender, GamepadAxisEventArgs e)
    {
        axisValues[e.Axis] = e.Value;
        connectedGamepadId = e.GamepadId;
        
        // Only log significant axis changes to avoid spam
        if (Math.Abs(e.Value) > 0.1f)
        {
            AddToLog($"Axis Motion: {e.Axis} = {e.Value:F3} (Gamepad {e.GamepadId})");
        }
    }

    private void AddToLog(string message)
    {
        var timestampedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        eventLog.Add(timestampedMessage);
        
        // Keep only the last 50 messages
        if (eventLog.Count > 50)
        {
            eventLog.RemoveAt(0);
        }
    }

    protected override void OnUpdate(double deltaSeconds)
    {
        // No special update logic needed for this test
    }

    protected override void OnRender(double deltaSeconds)
    {
        CommandProcessor.Submit(commandBuffer =>
        {
            SwapChain.FrameBuffer.BeginRenderPass(commandBuffer);

            // Clear the screen
            SwapChain.FrameBuffer.Clear(commandBuffer, new(0.1f, 0.1f, 0.1f, 1.0f));

            // Render ImGui UI
            ImGuiController.NewFrame(Window.Size, (float)deltaSeconds);

            RenderGamepadUI();

            ImGuiController.Render(commandBuffer);

            SwapChain.FrameBuffer.EndRenderPass(commandBuffer);
        });

        SwapChain.Present();
    }

    private void RenderGamepadUI()
    {
        ImGui.SetNextWindowPos(new(10, 10));
        ImGui.SetNextWindowSize(new(600, 700));
        
        if (ImGui.Begin("Gamepad Input Test", ImGuiWindowFlags.NoResize))
        {
            if (connectedGamepadId >= 0)
            {
                ImGui.Text($"Connected Gamepad ID: {connectedGamepadId}");
            }
            else
            {
                ImGui.TextColored(new(1.0f, 0.5f, 0.0f, 1.0f), "No gamepad detected");
                ImGui.Text("Connect a gamepad and press any button to start");
            }

            ImGui.Separator();

            // Button states
            if (ImGui.CollapsingHeader("Button States", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Columns(3, "ButtonColumns", true);
                
                foreach (var kvp in buttonStates)
                {
                    if (kvp.Key == GamepadButton.Unknown) continue;
                    
                    var color = kvp.Value ? new System.Numerics.Vector4(0.0f, 1.0f, 0.0f, 1.0f) : new System.Numerics.Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                    ImGui.TextColored(color, $"{kvp.Key}: {(kvp.Value ? "PRESSED" : "Released")}");
                    ImGui.NextColumn();
                }
                
                ImGui.Columns(1);
            }

            ImGui.Separator();

            // Axis values
            if (ImGui.CollapsingHeader("Axis Values", ImGuiTreeNodeFlags.DefaultOpen))
            {
                foreach (var kvp in axisValues)
                {
                    if (kvp.Key == GamepadAxis.Unknown) continue;
                    
                    ImGui.Text($"{kvp.Key}:");
                    ImGui.SameLine(120);
                    ImGui.ProgressBar((kvp.Value + 1.0f) / 2.0f, new(200, 20), $"{kvp.Value:F3}");
                }
            }

            ImGui.Separator();

            // Event log
            if (ImGui.CollapsingHeader("Event Log", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.Button("Clear Log"))
                {
                    eventLog.Clear();
                }

                ImGui.BeginChild("EventLog", new(0, 200), true);
                
                foreach (var logEntry in eventLog)
                {
                    ImGui.Text(logEntry);
                }
                
                // Auto-scroll to bottom
                if (ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
                {
                    ImGui.SetScrollHereY(1.0f);
                }
                
                ImGui.EndChild();
            }
        }
        
        ImGui.End();
    }

    protected override void OnUnload()
    {
        // Unsubscribe from events
        Window.GamepadButtonDown -= OnGamepadButtonDown;
        Window.GamepadButtonUp -= OnGamepadButtonUp;
        Window.GamepadAxisMotion -= OnGamepadAxisMotion;
    }
}