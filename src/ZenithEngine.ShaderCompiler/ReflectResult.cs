namespace ZenithEngine.ShaderCompiler;

public class ReflectResult
{
    private readonly ReflectResource[] resources = [];

    internal ReflectResult(params ReflectResource[][] resourceGroups)
    {
        List<ReflectResource> combinedResources = [];

        foreach (ReflectResource[] group in resourceGroups)
        {
            foreach (ReflectResource resource in group)
            {
                int index = combinedResources.FindIndex(0,
                                                        combinedResources.Count,
                                                        item => item.Name == resource.Name);

                if (index is -1)
                {
                    combinedResources.Add(resource);
                }
                else
                {
                    combinedResources[index] = new(resource.Stages | combinedResources[index].Stages,
                                                   resource.Type,
                                                   resource.Slot,
                                                   resource.Space,
                                                   resource.Name,
                                                   resource.Count);
                }
            }
        }

        resources = [.. combinedResources];
    }

    public ReflectResource this[string name, uint? setCount = null]
    {
        get
        {
            if (setCount.HasValue)
            {
                ReflectResource resource = resources.FirstOrDefault(item => item.Name == name);

                return new(resource.Stages,
                           resource.Type,
                           resource.Slot,
                           resource.Space,
                           resource.Name,
                           setCount.Value);
            }

            return resources.FirstOrDefault(item => item.Name == name);
        }
    }

    public static ReflectResult Merge(params ReflectResult[] layouts)
    {
        return new ReflectResult(layouts.Select(item => item.resources).ToArray());
    }
}
