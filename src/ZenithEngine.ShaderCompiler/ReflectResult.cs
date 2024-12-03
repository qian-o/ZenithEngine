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
                    combinedResources[index] = new(resource.Space,
                                                   resource.Name,
                                                   resource.Slot,
                                                   resource.Type,
                                                   resource.Stages | combinedResources[index].Stages,
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

                return new(resource.Space,
                           resource.Name,
                           resource.Slot,
                           resource.Type,
                           resource.Stages,
                           setCount.Value);
            }

            return resources.FirstOrDefault(item => item.Name == name);
        }
    }

    public ReflectResult Merge(ReflectResult layout)
    {
        return new(resources, layout.resources);
    }
}
