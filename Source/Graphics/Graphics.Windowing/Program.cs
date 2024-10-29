using Graphics.Windowing;
using Graphics.Windowing.Interfaces;

IWindow window = new SdlWindow();

window.Show();

WindowManager.Loop();