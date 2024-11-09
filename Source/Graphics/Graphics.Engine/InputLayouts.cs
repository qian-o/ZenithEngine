using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public class InputLayouts
{
    private readonly List<LayoutDesc> layouts = [];

    public LayoutDesc this[int index] { get => layouts[index]; set => layouts[index] = value; }

    public int Count => layouts.Count;

    public InputLayouts Add(LayoutDesc item)
    {
        layouts.Add(item);

        return this;
    }
}
