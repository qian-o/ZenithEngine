using ZenithEngine.Common.Enums;

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

    public ReflectResource this[string name, ShaderStages? stages = null, uint? count = null]
    {
        get
        {
            ReflectResource resource = resources.FirstOrDefault(item => item.Name == name);

            return new(stages ?? resource.Stages,
                       resource.Type,
                       resource.Slot,
                       resource.Space,
                       resource.Name,
                       count ?? resource.Count);
        }
    }

    public static ReflectResult Merge(params ReflectResult[] layouts)
    {
        return new([.. layouts.Select(static item => item.resources)]);
    }
}
