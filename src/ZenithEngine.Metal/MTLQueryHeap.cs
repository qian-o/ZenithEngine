using System;
using Metal;
using ZenithEngine.Common.Descriptions;
using ZenithEngine.Common.Enums;
using ZenithEngine.Common.Graphics;

namespace ZenithEngine.Metal;

internal class MTLQueryHeap : QueryHeap
{
    private readonly IMTLCounterSampleBuffer? counterSampleBuffer;
    private readonly IMTLBuffer? resultsBuffer;

    public MTLQueryHeap(GraphicsContext context,
                        ref readonly QueryHeapDesc desc) : base(context, in desc)
    {
        if (desc.Count == 0)
        {
            throw new ArgumentException("Query heap count must be greater than 0", nameof(desc));
        }

        // Metal uses counter sample buffers for queries
        // Check if counter sampling is supported
        if (!Context.Device.SupportsCounterSampling(MTLCounterSamplingPoint.AtStageBoundary))
        {
            throw new NotSupportedException("Counter sampling is not supported on this device");
        }

        // Create counter sample buffer based on query type
        using MTLCounterSampleBufferDescriptor counterDesc = new();
        counterDesc.SampleCount = desc.Count;
        counterDesc.StorageMode = MTLStorageMode.Shared; // Need shared for CPU access

        // Select counter set based on query type
        if (desc.Type == QueryType.Timestamp)
        {
            // Try to get timestamp counter set
            counterDesc.CounterSet = GetTimestampCounterSet();
        }
        else
        {
            // Occlusion queries not directly supported via counter sample buffers
            // Would need visibility result buffer instead
            throw new NotSupportedException($"Query type {desc.Type} is not currently supported in Metal backend");
        }

        counterSampleBuffer = Context.Device.CreateCounterSampleBuffer(counterDesc, out NSError? error);
        if (counterSampleBuffer is null || error is not null)
        {
            throw new InvalidOperationException($"Failed to create Metal counter sample buffer: {error?.LocalizedDescription}");
        }

        // Create a buffer to hold resolved results
        ulong bufferSize = desc.Count * sizeof(ulong);
        resultsBuffer = Context.Device.CreateBuffer(bufferSize, MTLResourceOptions.StorageModeShared);
    }

    private new MTLGraphicsContext Context => (MTLGraphicsContext)base.Context;

    public IMTLCounterSampleBuffer? CounterSampleBuffer => counterSampleBuffer;

    public IMTLBuffer? ResultsBuffer => resultsBuffer;

    public override void GetData(int startIndex, Span<ulong> data)
    {
        if (resultsBuffer is null)
        {
            return;
        }

        // Copy data from results buffer
        unsafe
        {
            ulong* ptr = (ulong*)resultsBuffer.Contents;
            if (ptr is null)
            {
                return;
            }

            for (int i = 0; i < data.Length && (startIndex + i) < Desc.Count; i++)
            {
                data[i] = ptr[startIndex + i];
            }
        }
    }

    protected override void SetName(string name)
    {
        counterSampleBuffer?.SetLabel(name);
        resultsBuffer?.SetLabel($"{name}_Results");
    }

    protected override void Destroy()
    {
        resultsBuffer?.Dispose();
        counterSampleBuffer?.Dispose();
    }

    private IMTLCounterSet GetTimestampCounterSet()
    {
        // Get available counter sets from the device
        IMTLCounterSet[] counterSets = Context.Device.CounterSets;
        
        // Look for timestamp counter set
        foreach (IMTLCounterSet counterSet in counterSets)
        {
            if (counterSet.Name.Contains("timestamp", StringComparison.OrdinalIgnoreCase))
            {
                return counterSet;
            }
        }

        // If no timestamp counter set found, try to use the first available
        if (counterSets.Length > 0)
        {
            return counterSets[0];
        }

        throw new NotSupportedException("No suitable counter set found for timestamp queries");
    }
}
