using Spssly.SpssDataset;
using System.Collections.Generic;

namespace Spssly.Compression
{
    public interface IDataReallocator
    {
        IList<IRawRecord> ReAllocateRecords(IEnumerable<IRawRecord> records);
    }
}
