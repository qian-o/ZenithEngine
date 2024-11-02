using Graphics.Engine;
using Graphics.Engine.Enums;

using Context context = Context.Create(Backend.Vulkan);

context.CreateDevice(true);

Console.ReadKey();
