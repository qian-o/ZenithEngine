namespace ZenithEngine.ShaderCompiler;

public class ReflectResourceLayout
{
    private readonly ReflectResource[] cache = [];

    internal ReflectResourceLayout(params ReflectResource[][] caches)
    {
        List<ReflectResource> resources = [];

        foreach (ReflectResource[] cache in caches)
        {
            foreach (ReflectResource resource in cache)
            {
                int index = resources.FindIndex(0,
                                                resources.Count,
                                                item => item.Name == resource.Name);

                if (index == -1)
                {
                    resources.Add(resource);
                }
                else
                {
                    resources[index] = new(resource.Space,
                                           resource.Name,
                                           resource.Slot,
                                           resource.Type,
                                           resource.Stages | resources[index].Stages,
                                           resource.Count);
                }
            }
        }

        cache = [.. resources];
    }

    public ReflectResource this[string name, uint? setCount = null]
    {
        get
        {
            if (setCount.HasValue)
            {
                ReflectResource resource = cache.FirstOrDefault(item => item.Name == name);

                return new(resource.Space,
                           resource.Name,
                           resource.Slot,
                           resource.Type,
                           resource.Stages,
                           setCount.Value);
            }

            return cache.FirstOrDefault(item => item.Name == name);
        }
    }

    public ReflectResourceLayout Merge(ReflectResourceLayout layout)
    {
        return new(cache, layout.cache);
    }
}
