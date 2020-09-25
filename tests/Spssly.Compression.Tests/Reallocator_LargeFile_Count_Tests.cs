using NUnit.Framework;
using Spssly.Compression.Tests.Helpers;
using Spssly.DataReader;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Spssly.Compression.Tests
{
    [TestFixture]
    public class Reallocator_LargeFile_Count_Tests
    {
        [Test]
        public void Reallocate_UncompressedFile_CopiesAllReordsAndVariables()
        {
            //after this test the new file is substantially smaller thean the original

            int originalRecordCount;
            int originalVariableCount;

            using (FileStream fileStream = new FileStream(@"TestFiles/230mbfile.sav.gz", FileMode.Open,
                    FileAccess.Read, FileShare.Read, 2048 * 10, FileOptions.SequentialScan))
            using (GZipStream deCompressStream = new GZipStream(fileStream, CompressionMode.Decompress, true))
            using (MemoryStream memStream = new MemoryStream())
            {
                deCompressStream.CopyTo(memStream);

                memStream.Position = 0;

                SpssReader spssDataset = new SpssReader(memStream);

                originalRecordCount = spssDataset.Records.Count();
                originalVariableCount = spssDataset.Variables.Count;

                var deallocator = new DataReallocator();

                // code under test
                var records = deallocator.ReAllocateRecords(spssDataset.Records);

                using (FileStream outputFileStream = new FileStream(@"TestFiles/34mbfile-after-reallocation.sav", FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new SpssWriter(outputFileStream, spssDataset.Variables, new SpssOptions() { Cases = originalRecordCount }))
                    {
                        foreach (var record in records)
                        {
                            var newRecord = writer.CreateRecord(record);
                            writer.WriteRecord(newRecord);
                        }
                        writer.EndFile();
                    }
                }
            }

            var results = FileValidator.CheckSpssFile(@"TestFiles/34mbfile-after-reallocation.sav");

            Assert.AreEqual(originalRecordCount, results.RecordCount);
            Assert.AreEqual(originalVariableCount, results.VariableCount);
        }

        [Test]
        public void Reallocate_PSPPCompressedFile_CopiesAllReordsAndVariables()
        {
            //after this test the new file is the same size as the original

            int originalRecordCount;
            int originalVariableCount;

            using (FileStream fileStream = new FileStream(@"TestFiles/34mbfile.sav.gz", FileMode.Open,
               FileAccess.Read, FileShare.Read, 2048 * 10, FileOptions.SequentialScan))
            using (GZipStream deCompressStream = new GZipStream(fileStream, CompressionMode.Decompress, true))
            using (MemoryStream memStream = new MemoryStream())
            {
                deCompressStream.CopyTo(memStream);
                memStream.Position = 0;

                SpssReader spssDataset = new SpssReader(memStream);

                originalRecordCount = spssDataset.Records.Count();
                originalVariableCount = spssDataset.Variables.Count();

                var deallocator = new DataReallocator();

                // code under test
                var records = deallocator.ReAllocateRecords(spssDataset.Records);

                var variables = spssDataset.Variables
                    .Where(x => x.Name == "qhidimpconcepteval" || x.Name == "qhidimpproduct1")
                    .ToList();

                var datalist = new List<string>();

                foreach (var record in records)
                {
                    foreach (var variable in variables)
                    {
                        var data = record.GetValue(variable)?.ToString()?.Trim();

                        if (!string.IsNullOrEmpty(data))
                            datalist.Add(data);
                    }
                }

                using (FileStream outputFileStream = new FileStream(@"4mbfile-after-reallocation.sav", FileMode.Create, FileAccess.Write))
                {
                    using (var writer = new SpssWriter(outputFileStream, spssDataset.Variables, new SpssOptions()))
                    {
                        foreach (var record in records)
                        {
                            var newRecord = writer.CreateRecord(record);
                            writer.WriteRecord(newRecord);
                        }
                        writer.EndFile();
                    }
                }
            }

            var results = FileValidator.CheckSpssFile(@"4mbfile-after-reallocation.sav");

            Assert.AreEqual(originalRecordCount, results.RecordCount);
            Assert.AreEqual(originalVariableCount, results.VariableCount);
        }
    }
}