# ZenithEngine Gamepad Support

This document describes the gamepad/controller support that has been added to ZenithEngine.

## Overview

ZenithEngine now supports gamepad input through SDL's joystick subsystem. The implementation follows the same event-driven architecture as keyboard and mouse input, providing a consistent developer experience.

## Features

### Supported Input Types
- **Buttons**: 21 gamepad buttons including face buttons (A, B, X, Y), D-pad, shoulder buttons, triggers, and special buttons
- **Analog Sticks**: Left and right stick X/Y axes with proper deadzone handling
- **Triggers**: Left and right trigger analog values
- **ImGui Navigation**: Full gamepad navigation support for UI elements

### Gamepad Buttons
- Face buttons: A, B, X, Y
- D-pad: Up, Down, Left, Right
- Shoulder buttons: Left/Right shoulder, Left/Right stick click
- System buttons: Back, Start, Guide
- Special buttons: Misc1, Paddle1-4, Touchpad

### Gamepad Axes
- Left stick: X and Y axes
- Right stick: X and Y axes
- Triggers: Left and right trigger pressure

## Usage

### Basic Event Handling

```csharp
// Subscribe to gamepad events
window.GamepadButtonDown += (sender, e) => {
    Console.WriteLine($"Button {e.Button} pressed on gamepad {e.GamepadId}");
};

window.GamepadButtonUp += (sender, e) => {
    Console.WriteLine($"Button {e.Button} released on gamepad {e.GamepadId}");
};

window.GamepadAxisMotion += (sender, e) => {
    Console.WriteLine($"Axis {e.Axis} moved to {e.Value:F3} on gamepad {e.GamepadId}");
};
```

### Camera Control with Gamepad

The `CameraController` class now automatically supports gamepad input:
- **Left stick**: Move forward/backward and strafe left/right
- **Right stick**: Look around (camera rotation)
- Deadzone handling prevents stick drift
- Smooth analog movement with speed scaling

### ImGui Integration

ImGui automatically supports gamepad navigation when gamepads are connected:
- Use D-pad or left stick to navigate UI elements
- A button to confirm/activate
- B button to cancel/go back
- Analog triggers and sticks work with appropriate UI elements

## Testing

Use the included `GamepadTest` example to verify gamepad functionality:

```bash
# Run the gamepad test (when building is supported)
dotnet run --project src/Examples/GamepadTest
```

The test application shows:
- Real-time button states (pressed/released)
- Analog axis values with visual progress bars
- Event log with timestamps
- Connected gamepad detection

## Implementation Details

### Architecture
- **Event-driven**: Follows the same pattern as keyboard/mouse input
- **SDL-based**: Leverages SDL's robust gamepad support
- **Cross-platform**: Works on Windows, Linux, and macOS
- **Multiple controllers**: Supports multiple connected gamepads

### Key Classes
- `GamepadButton` enum: Defines all supported button types
- `GamepadAxis` enum: Defines analog input axes
- `GamepadButtonEventArgs`: Button press/release event data
- `GamepadAxisEventArgs`: Analog input event data
- `IInput` interface: Extended with gamepad event signatures

### Integration Points
- **WindowUtils**: SDL initialization and input mapping
- **Window**: Event processing and forwarding
- **ImGuiController**: UI navigation support
- **CameraController**: 3D navigation enhancement

## Best Practices

1. **Always check for connected gamepads** before relying on gamepad input
2. **Use deadzone handling** for analog inputs to prevent drift
3. **Provide keyboard/mouse alternatives** for accessibility
4. **Test with different controller types** (Xbox, PlayStation, generic)
5. **Use the event-driven approach** rather than polling for consistency

## Troubleshooting

### Common Issues
- **No gamepad detected**: Ensure SDL joystick initialization succeeded
- **Button mapping incorrect**: Check if controller is recognized by SDL
- **Axis values jumpy**: Implement appropriate deadzone values
- **ImGui navigation not working**: Verify `NavEnableGamepad` is set

### Debugging
- Use the GamepadTest example to verify basic functionality
- Check SDL initialization logs for joystick subsystem
- Verify gamepad is recognized by the operating system
- Test with multiple controller types if available

## Future Enhancements

Potential improvements for future versions:
- Haptic feedback/vibration support
- Custom button mapping configuration
- Gamepad-specific UI themes
- Gesture recognition for advanced inputs
- Hot-plugging support with connection events

The gamepad support is now fully integrated into ZenithEngine and ready for use in your applications! 🎮