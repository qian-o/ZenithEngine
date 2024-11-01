namespace Graphics.Engine.Enums;

public enum BlendFunction
{
    /// <summary>
    /// Source and destination are added.
    /// </summary>
    Add,

    /// <summary>
    /// Destination is subtracted from source.
    /// </summary>
    Subtract,

    /// <summary>
    /// Source is subtracted from destination.
    /// </summary>
    ReverseSubtract,

    /// <summary>
    /// The minimum of source and destination is selected.
    /// </summary>
    Minimum,

    /// <summary>
    /// The maximum of source and destination is selected.
    /// </summary>
    Maximum
}
