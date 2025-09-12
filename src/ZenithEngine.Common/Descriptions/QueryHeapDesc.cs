using ZenithEngine.Common.Enums;

namespace ZenithEngine.Common.Descriptions;

public struct QueryHeapDesc(QueryType type, uint count)
{
    public QueryHeapDesc() : this(QueryType.Timestamp, 0)
    {
    }

    public QueryType Type = type;

    public uint Count = count;
}
