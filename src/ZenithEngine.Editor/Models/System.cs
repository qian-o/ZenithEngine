using System.Text.Json;
using ZenithEngine.Common.Enums;

namespace ZenithEngine.Editor.Models;

public class System
{
    private static readonly string FilePath = Path.Combine(AppContext.BaseDirectory, "system.json");

    public Backend Backend { get; set; } = Backend.Vulkan;

    public static System Load()
    {
        if (File.Exists(FilePath))
        {
            return JsonSerializer.Deserialize<System>(File.ReadAllText(FilePath)) ?? new();
        }

        return new();
    }

    public void Save()
    {
        File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
    }
}
