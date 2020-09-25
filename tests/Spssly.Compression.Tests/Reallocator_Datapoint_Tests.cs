using NUnit.Framework;
using Spssly.DataReader;
using Spssly.SpssDataset;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Spssly.Compression.Tests
{
    [TestFixture]
    public class Reallocator_Datapoint_Tests
    {
        [Test]
        public void ReAllocate_Allocates_Doubles_Ints_Strings()
        {
            var _reAllocator = new DataReallocator();

            ICollection<IRawRecord> newlyAllocatedRecords = new List<IRawRecord>();
            ICollection<Variable> variables = new List<Variable>();

            using (FileStream fileStream = new FileStream(@"TestFiles/reallocationtests.sav", FileMode.Open,
                    FileAccess.Read, FileShare.Read, 2048 * 10, FileOptions.SequentialScan))
            { 
                var reader = new SpssReader(fileStream);

                newlyAllocatedRecords = _reAllocator.ReAllocateRecords(reader.Records);

                variables = reader.Variables;
            }

            var doubleVariable = variables.SingleOrDefault(x => x.Name == "double");
            var intVariable = variables.SingleOrDefault(x => x.Name == "numeric");
            var string50charVariable = variables.SingleOrDefault(x => x.Name == "string50char");
            var string750charVariable = variables.SingleOrDefault(x => x.Name == "string750char");

            //doubles
            Assert.AreEqual(newlyAllocatedRecords.First().GetValue(doubleVariable), 1.12);
            Assert.AreEqual(newlyAllocatedRecords.Skip(1).First().GetValue(doubleVariable), 2.91);
            Assert.AreEqual(newlyAllocatedRecords.Last().GetValue(doubleVariable), null);

            //ints
            Assert.AreEqual(newlyAllocatedRecords.First().GetValue(intVariable), 1);
            Assert.AreEqual(newlyAllocatedRecords.Skip(1).First().GetValue(intVariable), null);
            Assert.AreEqual(newlyAllocatedRecords.Last().GetValue(intVariable), 3);

            //string with 50 char max size
            Assert.AreEqual(newlyAllocatedRecords.First().GetValue(string50charVariable), "tfdutq");
            Assert.AreEqual(newlyAllocatedRecords.Skip(1).First().GetValue(string50charVariable), "mhom");
            Assert.AreEqual(newlyAllocatedRecords.Last().GetValue(string50charVariable), null);

            //string with 750 char max size
            Assert.AreEqual(newlyAllocatedRecords.First().GetValue(string750charVariable), ".");
            Assert.AreEqual(newlyAllocatedRecords.Skip(1).First().GetValue(string750charVariable), "some random text");
            Assert.AreEqual(newlyAllocatedRecords.Last().GetValue(string750charVariable), null);
        }

        [Test]
        public void ReAllocate_Trims_Strings()
        {
            var _reAllocator = new DataReallocator();

            ICollection<IRawRecord> newlyAllocatedRecords = new List<IRawRecord>();
            ICollection<Variable> variables = new List<Variable>();

            using (FileStream fileStream = new FileStream(@"TestFiles/reallocationtests.sav", FileMode.Open,
                    FileAccess.Read, FileShare.Read, 2048 * 10, FileOptions.SequentialScan))
            {
                var reader = new SpssReader(fileStream);

                newlyAllocatedRecords = _reAllocator.ReAllocateRecords(reader.Records);

                variables = reader.Variables;
            }

            var string50charVariable = variables.SingleOrDefault(x => x.Name == "string50char");
            var string750charVariable = variables.SingleOrDefault(x => x.Name == "string750char");

            var emptySpaceRegex = new Regex(@"\s+$");

            //string with 50 char max size
            Assert.IsFalse(emptySpaceRegex.IsMatch(newlyAllocatedRecords.First().GetValue(string50charVariable)?.ToString()));
            Assert.IsFalse(emptySpaceRegex.IsMatch(newlyAllocatedRecords.Skip(1).First().GetValue(string50charVariable)?.ToString()));

            //string with 750 char max size
            Assert.IsFalse(emptySpaceRegex.IsMatch(newlyAllocatedRecords.First().GetValue(string750charVariable)?.ToString()));
            Assert.IsFalse(emptySpaceRegex.IsMatch(newlyAllocatedRecords.Skip(1).First().GetValue(string750charVariable)?.ToString()));
        }
    }
}

