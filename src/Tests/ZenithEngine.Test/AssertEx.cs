namespace ZenithEngine.Test;

internal class AssertEx
{
    public static string IsConsoleErrorEmpty(Action action)
    {
        using StringWriter writer = new();

        Console.SetOut(writer);

        action();

        string output = writer.ToString();

        Assert.IsFalse(output.ToLower().Contains("error", StringComparison.InvariantCulture));

        return output;
    }
}
