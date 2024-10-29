using Graphics.Windowing;
using Graphics.Windowing.Interactivity;

SdlWindow window = new();

window.KeyUp += KeyUp;

window.Show();

WindowManager.Loop();

static void KeyUp(object? sender, KeyEventArgs e)
{
    Console.WriteLine($"Key up: {e.Modifiers} - {e.Key}");
}