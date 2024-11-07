using Graphics.Engine.Descriptions;

namespace Graphics.Engine;

public abstract class GraphicsPipeline(Context context,
                                       ref readonly GraphicsPipelineDescription description) : Pipeline(context)
{
    public GraphicsPipelineDescription Description { get; } = description;
}
