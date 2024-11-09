using System.Collections;
using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public class InputLayouts : IList<LayoutDesc>
{
    private readonly List<LayoutDesc> layouts = [];

    public LayoutDesc this[int index] { get => layouts[index]; set => layouts[index] = value; }

    public int Count => layouts.Count;

    public bool IsReadOnly => false;

    public void Add(LayoutDesc item)
    {
        layouts.Add(item);
    }

    public void Clear()
    {
        layouts.Clear();
    }

    public bool Contains(LayoutDesc item)
    {
        return layouts.Contains(item);
    }

    public void CopyTo(LayoutDesc[] array, int arrayIndex)
    {
        layouts.CopyTo(array, arrayIndex);
    }

    public IEnumerator<LayoutDesc> GetEnumerator()
    {
        return layouts.GetEnumerator();
    }

    public int IndexOf(LayoutDesc item)
    {
        return layouts.IndexOf(item);
    }

    public void Insert(int index, LayoutDesc item)
    {
        layouts.Insert(index, item);
    }

    public bool Remove(LayoutDesc item)
    {
        return layouts.Remove(item);
    }

    public void RemoveAt(int index)
    {
        layouts.RemoveAt(index);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return layouts.GetEnumerator();
    }
}
