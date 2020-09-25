using Spssly.SpssDataset;
using System.Collections.Generic;
using System.Linq;

namespace Spssly.Compression
{
    public class DataReallocator : IDataReallocator
    {
        public IList<IRawRecord> ReAllocateRecords(IEnumerable<IRawRecord> records)
        {
            var adjustedRecords = new List<IRawRecord>();

            foreach (var record in records)
            {
                int count = record.Data.Count();

                for (int i = 0; i < count; i++)
                {
                    if (record.Data[i] is double convertableDouble)
                    {
                        record.Data[i] = convertableDouble;
                        continue;
                    }

                    record.Data[i] = record.Data[i]?.ToString().Trim();
                }

                adjustedRecords.Add(record);
            }

            return adjustedRecords;
        }
    }
}
