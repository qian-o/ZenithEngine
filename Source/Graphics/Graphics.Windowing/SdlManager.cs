using Silk.NET.SDL;

namespace Graphics.Windowing;

internal static unsafe class SdlManager
{
    public static readonly Sdl Sdl = Sdl.GetApi();

    public static List<Event> Events { get; } = [];

    public static void PollEvents()
    {
        Events.Clear();

        Event ev;
        while (Sdl.PollEvent(&ev) == (int)SdlBool.True)
        {
            Events.Add(ev);
        }
    }
}
