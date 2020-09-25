using Spssly.DataReader;
using System.IO;
using System.Linq;

namespace Spssly.Compression.Tests.Helpers
{
    public static class FileValidator
    {
        public static FileResults CheckSpssFile(string filePath)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read, 2048 * 10, FileOptions.SequentialScan))
            {
                SpssReader spssDataset = new SpssReader(fileStream);

                return new FileResults(spssDataset.Records.Count(), spssDataset.Variables.Count());
            }
        }
    }
}
