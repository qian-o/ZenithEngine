using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Events;

public class GamepadAxisEventArgs(int gamepadId, GamepadAxis axis, float value) : EventArgs
{
    public int GamepadId { get; } = gamepadId;

    public GamepadAxis Axis { get; } = axis;

    public float Value { get; } = value;
}